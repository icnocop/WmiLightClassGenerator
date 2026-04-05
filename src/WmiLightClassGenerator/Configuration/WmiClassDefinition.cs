// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Defines a single WMI class to generate a C# wrapper for.
/// </summary>
public sealed class WmiClassDefinition
{
    /// <summary>
    /// Gets or sets the WMI namespace (e.g. <c>root\virtualization\v2</c>).
    /// </summary>
    [JsonPropertyName("wmiNamespace")]
    public string WmiNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the WMI class name (e.g. <c>Msvm_ComputerSystem</c>).
    /// </summary>
    [JsonPropertyName("wmiClassName")]
    public string WmiClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the C# class name for the generated wrapper. If not specified,
    /// derived from the WMI class name by stripping the prefix.
    /// </summary>
    [JsonPropertyName("className")]
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the output file name. If not specified, derived from <see cref="ClassName"/>.
    /// </summary>
    [JsonPropertyName("outputFileName")]
    public string? OutputFileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether in-parameters of methods in this class
    /// should default to optional unless they have the <c>Required</c> qualifier.
    /// Use this for WMI classes that don't expose the <c>Required</c> qualifier but
    /// whose parameters are effectively optional at the COM level.
    /// </summary>
    [JsonPropertyName("defaultOptional")]
    public bool DefaultOptional { get; set; }

    /// <summary>
    /// Gets the effective C# class name.
    /// </summary>
    [JsonIgnore]
    public string EffectiveClassName =>
        this.ClassName ?? StripWmiPrefix(this.WmiClassName);

    /// <summary>
    /// Gets the effective output file name.
    /// </summary>
    [JsonIgnore]
    public string EffectiveOutputFileName =>
        this.OutputFileName ?? $"{this.EffectiveClassName}.cs";

    private static string StripWmiPrefix(string wmiClassName)
    {
        // Strip common prefixes: Msvm_, MSFT_, CIM_, Win32_
        int underscoreIndex = wmiClassName.IndexOf('_', StringComparison.Ordinal);
        if (underscoreIndex >= 0 && underscoreIndex < wmiClassName.Length - 1)
        {
            string prefix = wmiClassName[..underscoreIndex];
            if (prefix is "Msvm" or "MSFT" or "CIM" or "Win32")
            {
                return wmiClassName[(underscoreIndex + 1)..];
            }
        }

        return wmiClassName;
    }
}