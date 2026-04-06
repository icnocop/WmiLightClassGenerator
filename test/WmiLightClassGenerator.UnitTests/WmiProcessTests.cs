// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.UnitTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WmiLight;

/// <summary>
/// Tests for the generated <see cref="WmiProcess"/> WMI wrapper class.
/// </summary>
[TestClass]
public class WmiProcessTests
{
    /// <summary>
    /// Verifies that the GetOwner method can be called on the current process.
    /// </summary>
    [TestMethod]
    public void GetOwner_CanBeCalled()
    {
        using var connection = new WmiConnection();
        int currentProcessId = Environment.ProcessId;

        foreach (var proc in WmiProcess.Where(connection, p => p.ProcessId == (uint)currentProcessId))
        {
            using (proc)
            {
                uint result = proc.GetOwner(out string domain, out string user);

                Assert.AreEqual(0u, result, "GetOwner should return 0 for success.");
                Assert.IsFalse(string.IsNullOrEmpty(user), "User should not be null or empty.");
            }
        }
    }

    /// <summary>
    /// Verifies that filtering by ProcessId returns the current process.
    /// </summary>
    [TestMethod]
    public void Where_ByProcessId_ReturnsCurrentProcess()
    {
        using var connection = new WmiConnection();
        int currentProcessId = Environment.ProcessId;
        uint count = 0;

        foreach (var proc in WmiProcess.Where(connection, p => p.ProcessId == (uint)currentProcessId))
        {
            using (proc)
            {
                count++;
                Assert.AreEqual((uint)currentProcessId, proc.ProcessId, "ProcessId should match.");
                Assert.IsFalse(string.IsNullOrEmpty(proc.Name), "Name should not be null or empty.");
            }
        }

        Assert.AreEqual(1u, count, "Expected exactly one process matching the current process ID.");
    }
}
