// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.UnitTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WmiLight;

/// <summary>
/// Tests for the generated <see cref="OperatingSystem"/> WMI wrapper class.
/// </summary>
[TestClass]
public class OperatingSystemTests
{
    /// <summary>
    /// Verifies that <see cref="OperatingSystem.GetInstances(WmiLight.WmiConnection)"/> returns exactly one instance.
    /// </summary>
    [TestMethod]
    public void GetInstances_ReturnsExactlyOneInstance()
    {
        uint count = 0;

        using (var connection = new WmiConnection())
        {
            foreach (var os in OperatingSystem.GetInstances(connection))
            {
                count++;
                os.Dispose();
            }
        }

        Assert.AreEqual(1u, count, "Expected exactly one operating system instance.");
    }

    /// <summary>
    /// Verifies that the Caption property is not null or empty.
    /// </summary>
    [TestMethod]
    public void Caption_IsNotNullOrEmpty()
    {
        using var connection = new WmiConnection();
        foreach (var os in OperatingSystem.GetInstances(connection))
        {
            using (os)
            {
                Assert.IsFalse(
                    string.IsNullOrEmpty(os.Caption),
                    "Caption should not be null or empty.");
            }
        }
    }

    /// <summary>
    /// Verifies that the Version property is not null or empty.
    /// </summary>
    [TestMethod]
    public void Version_IsNotNullOrEmpty()
    {
        using var connection = new WmiConnection();
        foreach (var os in OperatingSystem.GetInstances(connection))
        {
            using (os)
            {
                Assert.IsFalse(
                    string.IsNullOrEmpty(os.Version),
                    "Version should not be null or empty.");
            }
        }
    }

    /// <summary>
    /// Verifies that the CSName property matches the local machine name.
    /// </summary>
    [TestMethod]
    public void CSName_MatchesMachineName()
    {
        using var connection = new WmiConnection();
        foreach (var os in OperatingSystem.GetInstances(connection))
        {
            using (os)
            {
                Assert.AreEqual(
                    Environment.MachineName,
                    os.CSName,
                    ignoreCase: true,
                    "CSName should match the local machine name.");
            }
        }
    }
}
