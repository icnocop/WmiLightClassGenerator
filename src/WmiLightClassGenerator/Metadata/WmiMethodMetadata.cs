// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

/// <summary>
/// Metadata for a single WMI method.
/// </summary>
public sealed class WmiMethodMetadata
{
    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input parameters.
    /// </summary>
    public List<WmiParameterMetadata> InParameters { get; set; } = [];

    /// <summary>
    /// Gets or sets the output parameters (including ReturnValue).
    /// </summary>
    public List<WmiParameterMetadata> OutParameters { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the method is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Gets or sets the method description from WMI qualifiers.
    /// </summary>
    public string? Description { get; set; }
}