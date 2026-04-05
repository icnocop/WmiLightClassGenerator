# WmiLightClassGenerator

[![Build](https://github.com/icnocop/WmiLightClassGenerator/actions/workflows/build.yml/badge.svg)](https://github.com/icnocop/WmiLightClassGenerator/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/WmiLightClassGenerator.svg)](https://www.nuget.org/packages/WmiLightClassGenerator)

Generates strongly-typed C# wrapper classes for Windows Management Instrumentation (WMI) classes using the [WmiLight](https://github.com/MartinKuschnik/WmiLight) library.

## Features

- Generates strongly-typed C# wrapper classes from WMI class definitions
- Auto-discovers enums from WMI `ValueMap`/`Values` qualifiers with inheritance chain traversal
- Supports string-backed enums with `[WmiValue]` attributes and bidirectional conversion extensions
- Generates static query helpers (`GetInstances`, `GetByKey`, `FromPath`, `Where`)
- Fluent `Builder` pattern for embedded instance XML construction
- Full XML documentation comments on all generated members
- Handles WMI method parameters (in/out/ref) with proper typing
- Reference class mappings for typed method return values
- Configurable via JSON config file
- Distributed as a build-only NuGet package (not added as a runtime dependency)

## Getting Started

### Installation

```shell
dotnet add package WmiLightClassGenerator
```

Since this is a build-only package, add `PrivateAssets="all"` to prevent it from becoming a transitive dependency:

```xml
<PackageReference Include="WmiLightClassGenerator" PrivateAssets="all" />
```

Your project will also need a reference to [WmiLight](https://www.nuget.org/packages/WmiLight) for the generated code:

```shell
dotnet add package WmiLight
```

### Configuration

Create a `wmi-classes.json` file in your project:

```json
{
  "outputDirectory": "Generated",
  "namespace": "MyProject.Wmi",
  "classes": [
    {
      "wmiNamespace": "root\\cimv2",
      "wmiClassName": "Win32_OperatingSystem",
      "className": "OperatingSystem"
    }
  ]
}
```

### Usage

Run the generator:

```shell
WmiLightClassGenerator.exe wmi-classes.json
```

Or, if the consuming project has a `$(WmiLightClassGeneratorExe)` MSBuild property available from the NuGet package:

```shell
$(WmiLightClassGeneratorExe) wmi-classes.json
```

This generates strongly-typed C# classes in the configured output directory.

## Configuration Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `outputDirectory` | string | `"."` | Directory for generated files (relative to config file) |
| `namespace` | string | `"MyProject.Wmi"` | C# namespace for generated code |
| `classes` | array | `[]` | WMI classes to generate wrappers for |
| `stringEnums` | array | `[]` | Custom string-backed enum definitions |
| `propertyTypeOverrides` | object | `{}` | Map property names to custom enum types |
| `enumRenames` | object | `{}` | Rename auto-discovered enums |
| `referenceClassMappings` | object | `{}` | Map WMI reference class names to C# class names |
| `additionalEnumValues` | object | `{}` | Merge additional values into auto-discovered enums |

### Class Definition

| Property | Required | Description |
|----------|----------|-------------|
| `wmiNamespace` | Yes | WMI namespace (e.g., `root\cimv2`) |
| `wmiClassName` | Yes | WMI class name (e.g., `Win32_OperatingSystem`) |
| `className` | No | C# class name (auto-derived by stripping common prefixes like `Win32_`, `Msvm_`, `CIM_`) |
| `outputFileName` | No | Output file name (defaults to `{className}.cs`) |

### String Enum Definition

```json
{
  "stringEnums": [
    {
      "name": "ExecutionState",
      "description": "The execution state of a managed element.",
      "values": {
        "Unknown": "Unknown",
        "Running": "Running",
        "Suspended": "Suspended"
      }
    }
  ]
}
```

## Generated Code

The generator produces the following for each WMI class:

### Infrastructure Files

The generator always emits these helper files that the generated code depends on:
`WmiValueAttribute`, `WqlWhereVisitor`, `WqlExpressionEvaluator`, `WmiPathParser`,
`WmiDtdSerializer`, `WmiInstanceBuilder`, `WmiJobHelper`.

### Wrapper Classes

- Read-only properties mapped to WMI properties with proper C# types
- Instance and static methods with typed parameters
- `GetInstances(connection)` — enumerate all WMI instances
- `GetByKey(connection, ...)` — query by key properties
- `FromPath(connection, path)` — resolve from WMI object path
- `Where(connection, predicate)` — LINQ-style queries translated to WQL
- Fluent `Builder` for embedded instance XML
- `IDisposable` implementation

### Enum Types

- **Integer-backed enums** from WMI `ValueMap`/`Values` qualifiers with proper backing types
- **String-backed enums** with `[WmiValue]` attributes and converter extension methods (`ToWmiString()`, `To{EnumName}()`)
- Auto-discovered from properties and method parameters with inheritance chain traversal

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
