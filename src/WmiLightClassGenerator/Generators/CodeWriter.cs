// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Generators;

using System.Text;

/// <summary>
/// Helper for writing indented C# source code with CRLF line endings.
/// </summary>
public sealed class CodeWriter
{
    private const string IndentString = "    ";
    private const string NewLine = "\r\n";

    private readonly StringBuilder sb = new();
    private int indentLevel;

    /// <summary>
    /// Escapes XML entities in raw text for safe inclusion in XML doc comments.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }

    /// <summary>
    /// Writes a line of code at the current indentation level.
    /// </summary>
    /// <param name="text">The line of text to write.</param>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter Line(string text)
    {
        this.WriteIndent();
        this.sb.Append(text);
        this.sb.Append(NewLine);
        return this;
    }

    /// <summary>
    /// Writes a blank line.
    /// </summary>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter BlankLine()
    {
        this.sb.Append(NewLine);
        return this;
    }

    /// <summary>
    /// Writes an opening brace and increases indentation.
    /// </summary>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter OpenBrace()
    {
        this.Line("{");
        this.indentLevel++;
        return this;
    }

    /// <summary>
    /// Decreases indentation and writes a closing brace.
    /// </summary>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter CloseBrace()
    {
        this.indentLevel--;
        this.Line("}");
        return this;
    }

    /// <summary>
    /// Decreases indentation and writes a closing brace with a suffix (e.g. "};").
    /// </summary>
    /// <param name="suffix">The suffix to append after the closing brace.</param>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter CloseBrace(string suffix)
    {
        this.indentLevel--;
        this.Line("}" + suffix);
        return this;
    }

    /// <summary>
    /// Writes an XML doc summary comment block.
    /// </summary>
    /// <param name="text">The summary text.</param>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter DocSummary(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return this;
        }

        text = SanitizeDocText(text);

        this.Line("/// <summary>");
        foreach (string line in WrapText(text, 80))
        {
            this.Line($"/// {line}");
        }

        this.Line("/// </summary>");
        return this;
    }

    /// <summary>
    /// Writes an XML doc param tag.
    /// </summary>
    /// <param name="paramName">The parameter name.</param>
    /// <param name="description">The parameter description.</param>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter DocParam(string paramName, string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return this;
        }

        string escaped = SanitizeDocText(description);

        this.Line($"/// <param name=\"{paramName}\">{escaped}</param>");
        return this;
    }

    /// <summary>
    /// Writes an XML doc returns tag.
    /// </summary>
    /// <param name="description">The return value description.</param>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter DocReturns(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            this.Line($"/// <returns>{description}</returns>");
        }

        return this;
    }

    /// <summary>
    /// Increases the indentation level.
    /// </summary>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter Indent()
    {
        this.indentLevel++;
        return this;
    }

    /// <summary>
    /// Decreases the indentation level.
    /// </summary>
    /// <returns>The current <see cref="CodeWriter"/> instance.</returns>
    public CodeWriter Outdent()
    {
        if (this.indentLevel > 0)
        {
            this.indentLevel--;
        }

        return this;
    }

    /// <summary>
    /// Gets the generated code as a string.
    /// </summary>
    /// <returns>The generated code.</returns>
    public override string ToString() => this.sb.ToString().TrimEnd('\r', '\n');

    /// <summary>
    /// Sanitizes text for use in XML doc comments by collapsing newlines.
    /// </summary>
    private static string SanitizeDocText(string text)
    {
        // Replace all forms of newlines with a single space
        text = text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

        // Collapse multiple spaces
        while (text.Contains("  ", StringComparison.Ordinal))
        {
            text = text.Replace("  ", " ", StringComparison.Ordinal);
        }

        return text.Trim();
    }

    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        // Simple word-wrapping
        if (text.Length <= maxWidth)
        {
            yield return text;
            yield break;
        }

        var words = text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (string word in words)
        {
            if (currentLine.Length > 0 && currentLine.Length + 1 + word.Length > maxWidth)
            {
                yield return currentLine.ToString();
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
            {
                currentLine.Append(' ');
            }

            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
        {
            yield return currentLine.ToString();
        }
    }

    private void WriteIndent()
    {
        for (int i = 0; i < this.indentLevel; i++)
        {
            this.sb.Append(IndentString);
        }
    }
}