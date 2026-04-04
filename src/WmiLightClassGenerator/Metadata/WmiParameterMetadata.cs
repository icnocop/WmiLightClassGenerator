// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Metadata;

using System.Management;

/// <summary>
/// Metadata for a WMI method parameter.
/// </summary>
public sealed class WmiParameterMetadata
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CIM type of the parameter.
    /// </summary>
    public CimType CimType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the parameter is an array.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Gets or sets the parameter description from WMI qualifiers.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the enum metadata if this parameter has ValueMap/Values qualifiers.
    /// </summary>
    public WmiEnumMetadata? EnumInfo { get; set; }

    /// <summary>
    /// Gets or sets the class name of the embedded type for embedded objects.
    /// </summary>
    public string? EmbeddedClassName { get; set; }

    /// <summary>
    /// Gets or sets the referenced WMI class name for <see cref="System.Management.CimType.Reference"/> parameters.
    /// Extracted from the <c>CIMTYPE</c> qualifier (e.g. <c>ref:CIM_ComputerSystem</c> yields <c>CIM_ComputerSystem</c>).
    /// </summary>
    public string? ReferenceClassName { get; set; }
}