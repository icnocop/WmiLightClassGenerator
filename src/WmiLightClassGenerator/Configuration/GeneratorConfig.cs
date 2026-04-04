// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Root configuration model for the WMI class generator.
/// Deserialized from wmi-classes.json.
/// </summary>
public sealed class GeneratorConfig
{
    /// <summary>
    /// Gets or sets the directory where generated .cs files are written.
    /// Relative to the config file location.
    /// </summary>
    [JsonPropertyName("outputDirectory")]
    public string OutputDirectory { get; set; } = ".";

    /// <summary>
    /// Gets or sets the C# namespace for all generated classes.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "MyProject.Wmi";

    /// <summary>
    /// Gets or sets the WMI classes to generate wrappers for.
    /// </summary>
    [JsonPropertyName("classes")]
    public List<WmiClassDefinition> Classes { get; set; } = [];

    /// <summary>
    /// Gets or sets the string-backed enum definitions for WMI properties that use
    /// string values rather than integer codes.
    /// </summary>
    [JsonPropertyName("stringEnums")]
    public List<StringEnumDefinition> StringEnums { get; set; } = [];

    /// <summary>
    /// Gets or sets the mapping of WMI property names to enum type names.
    /// When a property name matches a key, the generator uses the
    /// specified enum type instead of the raw WMI type.
    /// </summary>
    [JsonPropertyName("propertyTypeOverrides")]
    public Dictionary<string, string> PropertyTypeOverrides { get; set; } = new();

    /// <summary>
    /// Gets or sets the mapping of auto-discovered enum names to renamed enum names.
    /// Use this to avoid conflicts with C# reserved words or <c>System</c> types
    /// (e.g. <c>"Type": "VhdType"</c>).
    /// </summary>
    [JsonPropertyName("enumRenames")]
    public Dictionary<string, string> EnumRenames { get; set; } = new();

    /// <summary>
    /// Gets or sets the mapping of WMI class names (from <c>CIMTYPE</c> reference qualifiers)
    /// to generated C# class names. Used to generate typed method return values for
    /// reference output parameters (e.g. <c>"CIM_ComputerSystem": "ComputerSystem"</c>).
    /// </summary>
    [JsonPropertyName("referenceClassMappings")]
    public Dictionary<string, string> ReferenceClassMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets additional enum values to merge into auto-discovered enums.
    /// Used for vendor-specific values (e.g. Hyper-V extensions in the <c>0x8000..0xFFFF</c> range)
    /// that are not present in the CIM base class ValueMap.
    /// Keys are enum names; values are dictionaries of member name to integer value.
    /// </summary>
    [JsonPropertyName("additionalEnumValues")]
    public Dictionary<string, Dictionary<string, long>> AdditionalEnumValues { get; set; } = new();
}