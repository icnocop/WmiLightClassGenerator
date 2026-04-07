// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.UnitTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WmiLight;

/// <summary>
/// Tests for the generated <see cref="PnPEntity"/> WMI wrapper class.
/// </summary>
[TestClass]
public class PnPEntityTests
{
    /// <summary>
    /// Verifies that the GetDeviceProperties method can be called successfully.
    /// </summary>
    [TestMethod]
    public void GetDeviceProperties_CanBeCalled()
    {
        using var connection = new WmiConnection();
        foreach (var entity in PnPEntity.GetInstances(connection))
        {
            using (entity)
            {
                uint result = entity.GetDeviceProperties(out WmiObject[] deviceProperties);

                Assert.AreEqual(0u, result, "GetDeviceProperties should return 0 for success.");

                // Test done — we only need to verify the method can be called successfully.
                return;
            }
        }

        Assert.Inconclusive("No PnP entities found on this system.");
    }

    /// <summary>
    /// Verifies that GetDeviceProperties can be called with null device property keys.
    /// </summary>
    [TestMethod]
    public void GetDeviceProperties_WithNullKeys_CanBeCalled()
    {
        using var connection = new WmiConnection();
        foreach (var entity in PnPEntity.GetInstances(connection))
        {
            using (entity)
            {
                uint result = entity.GetDeviceProperties(
                    out WmiObject[] deviceProperties,
                    devicePropertyKeys: null);

                Assert.AreEqual(0u, result, "GetDeviceProperties with null keys should return 0 for success.");

                // Test done — we only need to verify the method can be called successfully.
                return;
            }
        }

        Assert.Inconclusive("No PnP entities found on this system.");
    }
}
