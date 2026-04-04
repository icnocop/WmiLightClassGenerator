namespace {0};

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

/// <summary>
/// Translates a LINQ <see cref="Expression{TDelegate}"/> predicate into a
/// WQL WHERE clause string. Supports comparison operators, logical operators,
/// string methods (<c>StartsWith</c>, <c>EndsWith</c>, <c>Contains</c>),
/// null checks, and enum values (both integer-backed and
/// <see cref="WmiValueAttribute"/>-based string enums).
/// </summary>
internal sealed class WqlWhereVisitor : ExpressionVisitor
{
    private readonly StringBuilder sb = new StringBuilder();

    private WqlWhereVisitor()
    {
    }

    /// <summary>
    /// Translates a strongly-typed predicate expression into a WQL WHERE clause.
    /// </summary>
    /// <typeparam name="T">The WMI wrapper class type.</typeparam>
    /// <param name="predicate">The predicate expression to translate.</param>
    /// <returns>A WQL WHERE clause string (without the <c>WHERE</c> keyword).</returns>
    public static string Translate<T>(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var evaluated = WqlExpressionEvaluator.PartialEval(predicate.Body);
        var visitor = new WqlWhereVisitor();
        visitor.Visit(evaluated);
        return visitor.sb.ToString();
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        this.sb.Append('(');
        this.Visit(node.Left);

        switch (node.NodeType)
        {
            case ExpressionType.Equal:
                if (IsNullConstant(node.Right))
                {
                    this.sb.Append(" IS NULL");
                    this.sb.Append(')');
                    return node;
                }

                this.sb.Append(" = ");
                break;
            case ExpressionType.NotEqual:
                if (IsNullConstant(node.Right))
                {
                    this.sb.Append(" IS NOT NULL");
                    this.sb.Append(')');
                    return node;
                }

                this.sb.Append(" != ");
                break;
            case ExpressionType.LessThan:
                this.sb.Append(" < ");
                break;
            case ExpressionType.LessThanOrEqual:
                this.sb.Append(" <= ");
                break;
            case ExpressionType.GreaterThan:
                this.sb.Append(" > ");
                break;
            case ExpressionType.GreaterThanOrEqual:
                this.sb.Append(" >= ");
                break;
            case ExpressionType.AndAlso:
                this.sb.Append(" AND ");
                break;
            case ExpressionType.OrElse:
                this.sb.Append(" OR ");
                break;
            default:
                throw new NotSupportedException($"Binary operator '{node.NodeType}' is not supported in WQL.");
        }

        this.Visit(node.Right);
        this.sb.Append(')');
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitUnary(UnaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Not:
                this.sb.Append("NOT (");
                this.Visit(node.Operand);
                this.sb.Append(')');
                break;
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                this.Visit(node.Operand);
                break;
            default:
                throw new NotSupportedException($"Unary operator '{node.NodeType}' is not supported in WQL.");
        }

        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is not null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            this.sb.Append(node.Member.Name);
            return node;
        }

        throw new NotSupportedException(
            $"Member '{node.Member.Name}' is not a direct property of the WMI class parameter. " +
            "Ensure captured variables are resolved by the partial evaluator.");
    }

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        this.AppendValue(node.Value, node.Type);
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(string))
        {
            return this.VisitStringMethod(node);
        }

        throw new NotSupportedException($"Method '{node.Method.Name}' is not supported in WQL.");
    }

    private static bool IsNullConstant(Expression expression)
    {
        return expression is ConstantExpression c && c.Value is null;
    }

    private static string EvaluateStringArgument(Expression argument)
    {
        if (argument is ConstantExpression c && c.Value is string s)
        {
            return s;
        }

        var lambda = Expression.Lambda<Func<string>>(argument);
        return lambda.Compile()();
    }

    private static string EscapeWqlString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'");
    }

    private Expression VisitStringMethod(MethodCallExpression node)
    {
        string methodName = node.Method.Name;

        if (node.Object is null || node.Arguments.Count < 1)
        {
            throw new NotSupportedException($"String method '{methodName}' requires an instance and one argument.");
        }

        string value = EvaluateStringArgument(node.Arguments[0]);
        string escaped = EscapeWqlString(value);

        this.Visit(node.Object);

        switch (methodName)
        {
            case "StartsWith":
                this.sb.Append($" LIKE '{escaped}%'");
                break;
            case "EndsWith":
                this.sb.Append($" LIKE '%{escaped}'");
                break;
            case "Contains":
                this.sb.Append($" LIKE '%{escaped}%'");
                break;
            default:
                throw new NotSupportedException($"String method '{methodName}' is not supported in WQL.");
        }

        return node;
    }

    private void AppendValue(object value, Type type)
    {
        if (value is null)
        {
            this.sb.Append("NULL");
            return;
        }

        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        if (underlyingType.IsEnum)
        {
            this.AppendEnumValue(value, underlyingType);
            return;
        }

        switch (value)
        {
            case string s:
                this.sb.Append('\'');
                this.sb.Append(EscapeWqlString(s));
                this.sb.Append('\'');
                break;
            case bool b:
                this.sb.Append(b ? "TRUE" : "FALSE");
                break;
            case char ch:
                this.sb.Append('\'');
                this.sb.Append(ch);
                this.sb.Append('\'');
                break;
            default:
                this.sb.Append(value);
                break;
        }
    }

    private void AppendEnumValue(object value, Type enumType)
    {
        string enumMemberName = Enum.GetName(enumType, value);
        if (enumMemberName is not null)
        {
            FieldInfo field = enumType.GetField(enumMemberName);
            WmiValueAttribute wmiAttr = field?.GetCustomAttributes(typeof(WmiValueAttribute), false)
                .Cast<WmiValueAttribute>()
                .FirstOrDefault();

            if (wmiAttr is not null)
            {
                this.sb.Append('\'');
                this.sb.Append(EscapeWqlString(wmiAttr.Value));
                this.sb.Append('\'');
                return;
            }
        }

        object numericValue = Convert.ChangeType(value, Enum.GetUnderlyingType(enumType));
        this.sb.Append(numericValue);
    }
}
