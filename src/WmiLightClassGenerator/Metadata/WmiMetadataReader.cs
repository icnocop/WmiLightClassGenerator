// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

using System.Management;

/// <summary>
/// Reads WMI class metadata (properties, methods, qualifiers) using System.Management.
/// </summary>
public sealed class WmiMetadataReader
{
    /// <summary>
    /// Reads metadata for a WMI class.
    /// </summary>
    /// <param name="wmiNamespace">The WMI namespace (e.g. <c>root\virtualization\v2</c>).</param>
    /// <param name="wmiClassName">The WMI class name (e.g. <c>Msvm_ComputerSystem</c>).</param>
    /// <returns>The class metadata.</returns>
    public static WmiClassMetadata ReadClass(string wmiNamespace, string wmiClassName)
    {
        var scope = new ManagementScope(wmiNamespace);
        scope.Connect();

        var classPath = new ManagementPath(wmiClassName);
        using var wmiClass = new ManagementClass(scope, classPath, null);
        wmiClass.Options.UseAmendedQualifiers = true; // Get descriptions

        var metadata = new WmiClassMetadata
        {
            WmiNamespace = wmiNamespace,
            WmiClassName = wmiClassName,
            SuperClassName = GetSuperClass(wmiClass),
            Description = GetQualifierValue(wmiClass.Qualifiers, "Description"),
        };

        ReadProperties(wmiClass, metadata);
        ReadMethods(wmiClass, metadata);

        return metadata;
    }

    private static void ReadProperties(ManagementClass wmiClass, WmiClassMetadata metadata)
    {
        foreach (PropertyData prop in wmiClass.Properties)
        {
            // Skip system properties (those starting with __)
            if (prop.Name.StartsWith("__", StringComparison.Ordinal))
            {
                continue;
            }

            var propMetadata = new WmiPropertyMetadata
            {
                Name = prop.Name,
                CimType = prop.Type,
                IsArray = prop.IsArray,
                IsKey = HasQualifier(prop.Qualifiers, "key"),
                IsReadOnly = HasQualifier(prop.Qualifiers, "read") && !HasQualifier(prop.Qualifiers, "write"),
                Description = GetQualifierValue(prop.Qualifiers, "Description"),
                EmbeddedClassName = GetQualifierValue(prop.Qualifiers, "EmbeddedInstance"),
            };

            // Check for ValueMap/Values qualifiers (enum generation).
            // Msvm_ subclasses often don't inherit ValueMap qualifiers from CIM_ base classes,
            // so fall back to parent classes if not found on the current class.
            var enumInfo = ReadEnumQualifiers(prop.Name, prop.Qualifiers, prop.Type)
                ?? ReadEnumQualifiersFromParentClasses(wmiClass, prop.Name, prop.Type);
            if (enumInfo is not null)
            {
                propMetadata.EnumInfo = enumInfo;
            }

            if (propMetadata.IsKey)
            {
                metadata.KeyProperties.Add(prop.Name);
            }

            metadata.Properties.Add(propMetadata);
        }
    }

    private static void ReadMethods(ManagementClass wmiClass, WmiClassMetadata metadata)
    {
        foreach (MethodData method in wmiClass.Methods)
        {
            var methodMetadata = new WmiMethodMetadata
            {
                Name = method.Name,
                IsStatic = HasQualifier(method.Qualifiers, "Static"),
                Description = GetQualifierValue(method.Qualifiers, "Description"),
            };

            if (method.InParameters is not null)
            {
                foreach (PropertyData param in method.InParameters.Properties)
                {
                    methodMetadata.InParameters.Add(ReadParameter(param));
                }
            }

            if (method.OutParameters is not null)
            {
                foreach (PropertyData param in method.OutParameters.Properties)
                {
                    methodMetadata.OutParameters.Add(ReadParameter(param));
                }
            }

            metadata.Methods.Add(methodMetadata);
        }
    }

    private static WmiParameterMetadata ReadParameter(PropertyData param)
    {
        var metadata = new WmiParameterMetadata
        {
            Name = param.Name,
            CimType = param.Type,
            IsArray = param.IsArray,
            Description = GetQualifierValue(param.Qualifiers, "Description"),
            EnumInfo = ReadEnumQualifiers(param.Name, param.Qualifiers, param.Type),
            EmbeddedClassName = GetQualifierValue(param.Qualifiers, "EmbeddedInstance"),
        };

        // For reference parameters, extract the referenced class from the CIMTYPE qualifier (e.g. "ref:CIM_ComputerSystem")
        if (param.Type == CimType.Reference)
        {
            string? cimTypeQualifier = GetQualifierValue(param.Qualifiers, "CIMTYPE");
            if (cimTypeQualifier is not null && cimTypeQualifier.StartsWith("ref:", StringComparison.OrdinalIgnoreCase))
            {
                metadata.ReferenceClassName = cimTypeQualifier[4..];
            }
        }

        return metadata;
    }

    private static WmiEnumMetadata? ReadEnumQualifiers(
        string propertyName,
        QualifierDataCollection qualifiers,
        CimType cimType)
    {
        string[]? valueMap = GetQualifierArrayValue(qualifiers, "ValueMap");
        string[]? values = GetQualifierArrayValue(qualifiers, "Values");

        if (valueMap is null || values is null || valueMap.Length == 0)
        {
            return null;
        }

        var enumValues = new Dictionary<string, string>();

        int count = Math.Min(valueMap.Length, values.Length);
        for (int i = 0; i < count; i++)
        {
            string rawValue = valueMap[i];
            string displayName = values[i];

            // Skip range entries like "..","0..65535","32768..65535"
            if (rawValue.Contains("..", StringComparison.Ordinal))
            {
                continue;
            }

            string memberName = SanitizeEnumMemberName(displayName);
            if (string.IsNullOrEmpty(memberName))
            {
                continue;
            }

            // Avoid duplicate member names
            if (enumValues.ContainsKey(memberName))
            {
                memberName = $"{memberName}_{rawValue}";
            }

            enumValues[memberName] = rawValue;
        }

        if (enumValues.Count == 0)
        {
            return null;
        }

        // Determine if string-backed or integer-backed
        bool isStringBacked = !int.TryParse(enumValues.Values.First(), out _);

        string? backingType = isStringBacked
            ? null
            : Generators.CimTypeMapper.ToCSharpType(cimType);

        return new WmiEnumMetadata
        {
            EnumName = propertyName,
            IsStringBacked = isStringBacked,
            BackingType = backingType,
            Values = enumValues,
        };
    }

    /// <summary>
    /// Walks the WMI class derivation chain to find ValueMap/Values qualifiers on a property
    /// that the subclass did not inherit. This is common for Msvm_ classes that inherit from CIM_ base classes.
    /// </summary>
    private static WmiEnumMetadata? ReadEnumQualifiersFromParentClasses(
        ManagementClass wmiClass,
        string propertyName,
        CimType cimType)
    {
        string[]? derivation;
        try
        {
            derivation = (string[])wmiClass["__DERIVATION"];
        }
        catch
        {
            return null;
        }

        if (derivation is null || derivation.Length == 0)
        {
            return null;
        }

        foreach (string parentClassName in derivation)
        {
            try
            {
                using var parentClass = new ManagementClass(wmiClass.Scope, new ManagementPath(parentClassName), null);
                parentClass.Options.UseAmendedQualifiers = true;

                PropertyData parentProp = parentClass.Properties[propertyName];
                var enumInfo = ReadEnumQualifiers(propertyName, parentProp.Qualifiers, cimType);
                if (enumInfo is not null)
                {
                    return enumInfo;
                }
            }
            catch (ManagementException)
            {
                // Property not found on this parent, or class not accessible — continue up the chain
            }
        }

        return null;
    }

    private static string? GetSuperClass(ManagementClass wmiClass)
    {
        try
        {
            var derivation = (string[])wmiClass["__DERIVATION"];
            return derivation?.Length > 0 ? derivation[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool HasQualifier(QualifierDataCollection qualifiers, string name)
    {
        try
        {
            _ = qualifiers[name];
            return true;
        }
        catch (ManagementException)
        {
            return false;
        }
    }

    private static string? GetQualifierValue(QualifierDataCollection qualifiers, string name)
    {
        try
        {
            object? val = qualifiers[name]?.Value;
            return val?.ToString();
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static string[]? GetQualifierArrayValue(QualifierDataCollection qualifiers, string name)
    {
        try
        {
            object? val = qualifiers[name]?.Value;
            return val as string[];
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    /// <summary>
    /// Sanitizes a WMI Values entry into a valid C# enum member name.
    /// </summary>
    private static string SanitizeEnumMemberName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        // Replace common separators with nothing or underscore
        var result = new System.Text.StringBuilder(displayName.Length);
        bool capitalizeNext = true;

        foreach (char c in displayName)
        {
            if (char.IsLetterOrDigit(c))
            {
                result.Append(capitalizeNext ? char.ToUpperInvariant(c) : c);
                capitalizeNext = false;
            }
            else if (c is ' ' or '-' or '/' or '_' or '(' or ')')
            {
                capitalizeNext = true;
            }
        }

        string name = result.ToString();

        // C# identifiers cannot start with a digit
        if (name.Length > 0 && char.IsDigit(name[0]))
        {
            name = "_" + name;
        }

        return name;
    }
}