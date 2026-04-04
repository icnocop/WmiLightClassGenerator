// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Defines a string-backed enum for WMI properties whose values are strings.
/// </summary>
public sealed class StringEnumDefinition
{
    /// <summary>
    /// Gets or sets the C# enum type name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description for the XML doc comment.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the mapping of C# enum member names to their WMI string values.
    /// Key = enum member name, Value = WMI string value.
    /// </summary>
    [JsonPropertyName("values")]
    public Dictionary<string, string> Values { get; set; } = new();
}