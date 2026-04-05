// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

#pragma warning disable CA1303 // Do not pass literals as localized parameters - CLI tool

using System.Text;
using System.Text.Json;
using WmiLightClassGenerator.Configuration;
using WmiLightClassGenerator.Generators;
using WmiLightClassGenerator.Metadata;

/// <summary>
/// WMI Class Generator — generates strongly-typed C# wrappers for WMI classes using WmiLight.
/// </summary>
/// <remarks>
/// Usage: WmiLightClassGenerator [path-to-wmi-classes.json]
/// If no config path is provided, looks for wmi-classes.json in the current directory.
/// </remarks>
internal sealed class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private static int Main(string[] args)
    {
        try
        {
            string configPath = ResolveConfigPath(args);
            Console.WriteLine($"Reading config: {configPath}");

            var config = LoadConfig(configPath);
            string outputDir = ResolveOutputDir(configPath, config.OutputDirectory);

            Console.WriteLine($"Output directory: {outputDir}");
            Console.WriteLine($"Namespace: {config.Namespace}");
            Console.WriteLine($"Classes to generate: {config.Classes.Count}");
            Console.WriteLine();

            // Emit infrastructure files that generated code depends on
            Console.WriteLine("Emitting infrastructure files...");
            InfrastructureEmitter.EmitAll(outputDir, config.Namespace, WriteFile);

            var enumGen = new EnumGenerator(config.Namespace);
            var classGen = new ClassGenerator(
                config.Namespace,
                config.PropertyTypeOverrides,
                config.StringEnums.Select(e => e.Name),
                config.ReferenceClassMappings);

            // Track generated enums to avoid duplicates
            var generatedEnums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Generate string enums from config
            foreach (var stringEnumDef in config.StringEnums)
            {
                string enumCode = enumGen.GenerateStringEnum(stringEnumDef);
                string enumFile = Path.Combine(outputDir, $"{stringEnumDef.Name}.cs");
                WriteFile(enumFile, enumCode);
                generatedEnums.Add(stringEnumDef.Name);
                Console.WriteLine($"  Generated string enum: {stringEnumDef.Name}.cs");
            }

            // Generate classes
            foreach (var classDef in config.Classes)
            {
                Console.Write($"  {classDef.WmiClassName} -> {classDef.EffectiveClassName}.cs ... ");

                try
                {
                    var metadata = WmiMetadataReader.ReadClass(classDef.WmiNamespace, classDef.WmiClassName);
                    string className = classDef.EffectiveClassName;

                    // Apply enum renames from config
                    ApplyEnumRenames(metadata, config.EnumRenames);

                    // Generate enums discovered from ValueMap qualifiers
                    foreach (var prop in metadata.Properties)
                    {
                        if (prop.EnumInfo is not null && !generatedEnums.Contains(prop.EnumInfo.EnumName))
                        {
                            // Skip if this enum is already a config-defined string enum
                            if (config.PropertyTypeOverrides.ContainsKey(prop.Name))
                            {
                                continue;
                            }

                            // Merge additional enum values from config (e.g. vendor-specific extensions)
                            MergeAdditionalEnumValues(prop.EnumInfo, config.AdditionalEnumValues);

                            string enumCode = prop.EnumInfo.IsStringBacked
                                ? enumGen.GenerateStringEnum(prop.EnumInfo)
                                : enumGen.Generate(prop.EnumInfo);

                            string enumFile = Path.Combine(outputDir, $"{prop.EnumInfo.EnumName}.cs");
                            WriteFile(enumFile, enumCode);
                            generatedEnums.Add(prop.EnumInfo.EnumName);
                            Console.Write($"[enum:{prop.EnumInfo.EnumName}] ");
                        }
                    }

                    // Also check method parameters for enums
                    foreach (var method in metadata.Methods)
                    {
                        foreach (var param in method.InParameters.Concat(method.OutParameters))
                        {
                            if (param.EnumInfo is not null && !generatedEnums.Contains(param.EnumInfo.EnumName))
                            {
                                string enumCode = param.EnumInfo.IsStringBacked
                                    ? enumGen.GenerateStringEnum(param.EnumInfo)
                                    : enumGen.Generate(param.EnumInfo);

                                string enumFile = Path.Combine(outputDir, $"{param.EnumInfo.EnumName}.cs");
                                WriteFile(enumFile, enumCode);
                                generatedEnums.Add(param.EnumInfo.EnumName);
                                Console.Write($"[enum:{param.EnumInfo.EnumName}] ");
                            }
                        }
                    }

                    // Generate the wrapper class
                    string classCode = classGen.Generate(metadata, className, classDef);
                    string classFile = Path.Combine(outputDir, classDef.EffectiveOutputFileName);
                    WriteFile(classFile, classCode);

                    Console.WriteLine("OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                    Console.Error.WriteLine($"  Error generating {classDef.WmiClassName}: {ex}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Generation complete. {config.Classes.Count} classes processed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static string ResolveConfigPath(string[] args)
    {
        if (args.Length > 0)
        {
            return Path.GetFullPath(args[0]);
        }

        // Look in current directory
        string localPath = Path.Combine(Environment.CurrentDirectory, "wmi-classes.json");
        if (File.Exists(localPath))
        {
            return localPath;
        }

        throw new FileNotFoundException(
            "Config file not found. Provide a path argument or place wmi-classes.json in the current directory.");
    }

    private static GeneratorConfig LoadConfig(string configPath)
    {
        string json = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<GeneratorConfig>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize config file.");
    }

    private static string ResolveOutputDir(string configPath, string outputDirectory)
    {
        string configDir = Path.GetDirectoryName(configPath) ?? Environment.CurrentDirectory;
        string outputDir = Path.GetFullPath(Path.Combine(configDir, outputDirectory));
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    private static void WriteFile(string path, string content)
    {
        string? dir = Path.GetDirectoryName(path);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        byte[] newBytes = encoding.GetBytes(content);

        if (File.Exists(path))
        {
            byte[] existingBytes = File.ReadAllBytes(path);
            if (existingBytes.AsSpan().SequenceEqual(newBytes))
            {
                return; // unchanged
            }

            var attrs = File.GetAttributes(path);
            if (attrs.HasFlag(FileAttributes.ReadOnly))
            {
                throw new IOException(
                    $"Cannot write to '{path}': file is read-only and content has changed.");
            }
        }

        File.WriteAllBytes(path, newBytes);
    }

    private static void ApplyEnumRenames(WmiClassMetadata metadata, Dictionary<string, string> renames)
    {
        if (renames.Count == 0)
        {
            return;
        }

        foreach (var prop in metadata.Properties)
        {
            if (prop.EnumInfo is not null && renames.TryGetValue(prop.EnumInfo.EnumName, out string? newName))
            {
                prop.EnumInfo.EnumName = newName;
            }
        }

        foreach (var method in metadata.Methods)
        {
            foreach (var param in method.InParameters.Concat(method.OutParameters))
            {
                if (param.EnumInfo is not null && renames.TryGetValue(param.EnumInfo.EnumName, out string? newName))
                {
                    param.EnumInfo.EnumName = newName;
                }
            }
        }
    }

    private static void MergeAdditionalEnumValues(
        WmiEnumMetadata enumInfo,
        Dictionary<string, Dictionary<string, long>> additionalValues)
    {
        if (!additionalValues.TryGetValue(enumInfo.EnumName, out var extraValues))
        {
            return;
        }

        foreach (var (name, value) in extraValues)
        {
            enumInfo.Values[name] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}