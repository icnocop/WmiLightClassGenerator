// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

/// <summary>
/// Metadata for a WMI class, captured from the WMI schema.
/// </summary>
public sealed class WmiClassMetadata
{
    /// <summary>
    /// Gets or sets the WMI namespace (e.g. <c>root\virtualization\v2</c>).
    /// </summary>
    public string WmiNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the WMI class name (e.g. <c>Msvm_ComputerSystem</c>).
    /// </summary>
    public string WmiClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the superclass name, if any.
    /// </summary>
    public string? SuperClassName { get; set; }

    /// <summary>
    /// Gets or sets the class description from WMI qualifiers.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets all properties defined on this class.
    /// </summary>
    public List<WmiPropertyMetadata> Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets all methods defined on this class.
    /// </summary>
    public List<WmiMethodMetadata> Methods { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of key properties (used for instance identification).
    /// </summary>
    public List<string> KeyProperties { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether any in-parameter across all methods has the
    /// <c>Required</c> qualifier. When true, the class uses the <c>Required</c> qualifier
    /// convention, meaning parameters without <c>Required</c> are implicitly optional.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool UsesRequiredQualifier =>
        this.Methods.Any(m =>
            m.InParameters.Any(p => p.IsRequired));
}