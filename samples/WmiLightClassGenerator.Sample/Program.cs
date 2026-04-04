// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

using WmiLight;
using WmiLightClassGenerator.Sample;
using OperatingSystem = WmiLightClassGenerator.Sample.OperatingSystem;

using var connection = new WmiConnection(@"\\.\root\cimv2");

// Display operating system information
Console.WriteLine("=== Operating System ===");
foreach (var os in OperatingSystem.GetInstances(connection))
{
    Console.WriteLine($"  Caption: {os.Caption}");
    Console.WriteLine($"  Version: {os.Version}");
    Console.WriteLine($"  OSArchitecture: {os.OSArchitecture}");
    Console.WriteLine($"  TotalVisibleMemorySize: {os.TotalVisibleMemorySize} KB");
    os.Dispose();
}

Console.WriteLine();

// Display current process owner using WMI method invocation
Console.WriteLine("=== Current Process Owner ===");
int currentProcessId = Environment.ProcessId;
foreach (var proc in WmiProcess.Where(connection, p => p.ProcessId == (uint)currentProcessId))
{
    Console.WriteLine($"  ProcessId: {proc.ProcessId}");
    Console.WriteLine($"  Name: {proc.Name}");
    uint result = proc.GetOwner(out string domain, out string user);
    if (result == 0)
    {
        Console.WriteLine($"  Owner: {domain}\\{user}");
    }

    proc.Dispose();
}
