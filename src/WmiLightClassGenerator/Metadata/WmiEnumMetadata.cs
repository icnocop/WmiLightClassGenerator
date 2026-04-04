// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

/// <summary>
/// Metadata for an enumeration discovered from WMI ValueMap/Values qualifiers.
/// </summary>
public sealed class WmiEnumMetadata
{
    /// <summary>
    /// Gets or sets the generated enum type name.
    /// </summary>
    public string EnumName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the enum is string-backed or integer-backed.
    /// </summary>
    public bool IsStringBacked { get; set; }

    /// <summary>
    /// Gets or sets the C# backing type for integer-backed enums (e.g. "ushort", "uint").
    /// Null for string-backed enums.
    /// </summary>
    public string? BackingType { get; set; }

    /// <summary>
    /// Gets or sets the mapping of enum member names to their values.
    /// For integer-backed enums: member name to integer string (e.g. "2", "3").
    /// For string-backed enums: member name to WMI string value.
    /// </summary>
    public Dictionary<string, string> Values { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional description for the enum's XML doc comment.
    /// </summary>
    public string? Description { get; set; }
}