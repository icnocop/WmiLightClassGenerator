// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.Generators;

using System.Management;

/// <summary>
/// Maps CIM (Common Information Model) types to C# type strings.
/// </summary>
public static class CimTypeMapper
{
    /// <summary>
    /// Maps a CIM type to its C# type string.
    /// </summary>
    /// <param name="cimType">The CIM type.</param>
    /// <param name="isArray">Whether the type is an array.</param>
    /// <returns>The C# type string (e.g. "uint", "string", "bool").</returns>
    public static string ToCSharpType(CimType cimType, bool isArray = false)
    {
        string baseType = cimType switch
        {
            CimType.Boolean => "bool",
            CimType.String => "string",
            CimType.UInt8 => "byte",
            CimType.SInt8 => "sbyte",
            CimType.UInt16 => "ushort",
            CimType.SInt16 => "short",
            CimType.UInt32 => "uint",
            CimType.SInt32 => "int",
            CimType.UInt64 => "ulong",
            CimType.SInt64 => "long",
            CimType.Real32 => "float",
            CimType.Real64 => "double",
            CimType.DateTime => "string",  // DMTF string; callers convert via DmtfDateTimeConverter
            CimType.Char16 => "char",
            CimType.Reference => "string", // WMI object path
            CimType.Object => "string",    // Embedded WMI object (passed as XML string)
            _ => "object",
        };

        return isArray ? $"{baseType}[]" : baseType;
    }

    /// <summary>
    /// Maps a CIM type to its DTD 2.0 type string for use in embedded instance XML.
    /// </summary>
    /// <param name="cimType">The CIM type.</param>
    /// <returns>The CIM type string (e.g. "string", "uint16", "boolean").</returns>
    public static string ToCimTypeString(CimType cimType)
    {
        return cimType switch
        {
            CimType.Boolean => "boolean",
            CimType.String => "string",
            CimType.UInt8 => "uint8",
            CimType.SInt8 => "sint8",
            CimType.UInt16 => "uint16",
            CimType.SInt16 => "sint16",
            CimType.UInt32 => "uint32",
            CimType.SInt32 => "sint32",
            CimType.UInt64 => "uint64",
            CimType.SInt64 => "sint64",
            CimType.Real32 => "real32",
            CimType.Real64 => "real64",
            CimType.DateTime => "datetime",
            CimType.Char16 => "char16",
            CimType.Reference => "string",
            CimType.Object => "string",
            _ => "string",
        };
    }

    /// <summary>
    /// Gets the WmiLight generic type parameter for GetPropertyValue&lt;T&gt;().
    /// For some CIM types, the WmiLight return type differs from the
    /// logical C# type (e.g., DateTime is stored as a DMTF string).
    /// </summary>
    /// <param name="cimType">The CIM type.</param>
    /// <param name="isArray">Whether the type is an array.</param>
    /// <returns>The WmiLight property type string.</returns>
    public static string ToWmiLightPropertyType(CimType cimType, bool isArray = false)
    {
        // WmiLight returns DMTF datetime strings as string
        // and references as string. Object is returned as object.
        return ToCSharpType(cimType, isArray);
    }

    /// <summary>
    /// Returns true if the CIM type is a value type in C#.
    /// </summary>
    /// <param name="cimType">The CIM type.</param>
    /// <returns><see langword="true"/> if the CIM type maps to a C# value type; otherwise, <see langword="false"/>.</returns>
    public static bool IsValueType(CimType cimType)
    {
        return cimType switch
        {
            CimType.String => false,
            CimType.DateTime => false, // Maps to string (DMTF format)
            CimType.Reference => false,
            CimType.Object => false,
            _ => true,
        };
    }
}