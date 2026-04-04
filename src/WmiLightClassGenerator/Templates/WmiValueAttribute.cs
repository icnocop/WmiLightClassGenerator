namespace {0};

using System;

/// <summary>
/// Maps an enum member to its WMI string representation.
/// Used for string-backed WMI enumerations where the WMI property
/// value is a string rather than an integer.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class WmiValueAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WmiValueAttribute"/> class.
    /// </summary>
    /// <param name="value">The WMI string value this enum member represents.</param>
    public WmiValueAttribute(string value) => this.Value = value;

    /// <summary>
    /// Gets the WMI string value this enum member represents.
    /// </summary>
    public string Value { get; }
}
