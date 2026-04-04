# WmiLightClassGenerator.Sample

This sample project demonstrates how to use the **WmiLightClassGenerator** tool to generate strongly-typed C# wrapper classes for WMI classes.

## Configuration

The `wmi-classes.json` file defines which WMI classes to generate:

```json
{
  "outputDirectory": "Generated",
  "namespace": "WmiLightClassGenerator.Sample",
  "classes": [
    {
      "wmiNamespace": "root\\cimv2",
      "wmiClassName": "Win32_OperatingSystem",
      "className": "OperatingSystem"
    },
    {
      "wmiNamespace": "root\\cimv2",
      "wmiClassName": "Win32_Process",
      "className": "WmiProcess"
    }
  ]
}
```

## Regenerating Code

Run the included script from this directory:

```shell
generate.cmd
```

This regenerates the `Generated\` folder with strongly-typed wrapper classes for the configured WMI classes.

## What It Demonstrates

- **Win32_OperatingSystem**: Reading WMI properties (Caption, Version, OSArchitecture, TotalVisibleMemorySize)
- **Win32_Process**: Invoking a WMI method (`GetOwner`) that returns data without modifying the system
