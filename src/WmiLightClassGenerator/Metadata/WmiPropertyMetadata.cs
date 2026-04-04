// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

using System.Management;

/// <summary>
/// Metadata for a single WMI property.
/// </summary>
public sealed class WmiPropertyMetadata
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CIM type of the property.
    /// </summary>
    public CimType CimType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is an array.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a key property.
    /// </summary>
    public bool IsKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the property description from WMI qualifiers.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the enum metadata if this property has ValueMap/Values qualifiers.
    /// </summary>
    public WmiEnumMetadata? EnumInfo { get; set; }

    /// <summary>
    /// Gets or sets the class name of the embedded type for embedded objects (CimType.Object).
    /// </summary>
    public string? EmbeddedClassName { get; set; }
}