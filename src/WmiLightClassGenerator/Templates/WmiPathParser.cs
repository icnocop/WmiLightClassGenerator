namespace {0};

using System.Text.RegularExpressions;

/// <summary>
/// Parses key property values from WMI object path strings.
/// WMI paths follow the format: <c>\\SERVER\namespace:ClassName.Key="Value"</c>.
/// </summary>
internal static class WmiPathParser
{
    /// <summary>
    /// Extracts a key property value from a WMI object path.
    /// </summary>
    /// <param name="wmiPath">The WMI object path.</param>
    /// <param name="keyName">The key property name to extract.</param>
    /// <returns>The extracted key value, or <see langword="null"/> if not found.</returns>
    public static string ExtractKeyValue(string wmiPath, string keyName)
    {
        if (string.IsNullOrEmpty(wmiPath))
        {
            return null;
        }

        string pattern = "(?<=[.,])" + Regex.Escape(keyName) + "=\"([^\"]+)\"";
        var match = Regex.Match(wmiPath, pattern);
        return match.Success ? match.Groups[1].Value.Replace("\\\\", "\\") : null;
    }
}
