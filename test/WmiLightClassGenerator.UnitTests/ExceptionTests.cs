// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.UnitTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WmiLight;

#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed

/// <summary>
/// Tests for error handling in the generated WMI wrapper classes.
/// </summary>
[TestClass]
public class ExceptionTests
{
    /// <summary>
    /// Verifies that the OperatingSystem constructor throws when connection is null.
    /// </summary>
    [TestMethod]
    public void OperatingSystem_Constructor_ThrowsOnNullConnection()
    {
        using var connection = new WmiConnection();
        using var wmiObject = GetFirstWmiObject(connection, OperatingSystem.WmiClassName);
        Assert.ThrowsException<ArgumentNullException>(
            () => new OperatingSystem(null!, wmiObject));
    }

    /// <summary>
    /// Verifies that the OperatingSystem constructor throws when WMI object is null.
    /// </summary>
    [TestMethod]
    public void OperatingSystem_Constructor_ThrowsOnNullWmiObject()
    {
        using var connection = new WmiConnection();
        Assert.ThrowsException<ArgumentNullException>(
            () => new OperatingSystem(connection, null!));
    }

    /// <summary>
    /// Verifies that querying with an invalid namespace connection throws
    /// <see cref="InvalidNamespaceException"/>.
    /// </summary>
    [TestMethod]
    public void GetInstances_WithInvalidNamespaceConnection_ThrowsException()
    {
        Assert.ThrowsException<InvalidNamespaceException>(() =>
        {
            using var connection = new WmiConnection("INVALID");
            foreach (var os in OperatingSystem.GetInstances(connection))
            {
                os.Dispose();
                Assert.Fail("Should not reach here.");
            }
        });
    }

    private static WmiObject GetFirstWmiObject(WmiConnection connection, string className)
    {
        var query = new WmiQuery(connection, $"SELECT * FROM {className}");
        using var enumerator = query.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Assert.Inconclusive($"No instances of {className} found.");
        }

        return enumerator.Current;
    }
}
