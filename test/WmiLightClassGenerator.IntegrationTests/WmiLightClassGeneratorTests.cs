// Copyright (c) WmiLightClassGenerator Contributors. Licensed under the MIT License.

namespace WmiLightClassGenerator.IntegrationTests;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class WmiLightClassGeneratorTests
{
    private static readonly string ExePath = Path.Combine(
        AppContext.BaseDirectory, "WmiLightClassGenerator.exe");

    [TestMethod]
    public async Task NoConfigFile_ReturnsErrorAndExitCode1()
    {
        using var tempDir = new TempDirectory();

        var (exitCode, stdout, stderr) = await RunGeneratorAsync(tempDir.Path);

        Assert.AreEqual(1, exitCode, $"Expected exit code 1. stdout: {stdout}\nstderr: {stderr}");
        StringAssert.Contains(
            stderr,
            "Config file not found. Provide a path argument or place wmi-classes.json in the current directory.");
    }

    [TestMethod]
    public async Task EmptyFile_ReturnsJsonParseErrorAndExitCode1()
    {
        using var tempDir = new TempDirectory();
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "wmi-classes.json"), string.Empty);

        var (exitCode, stdout, stderr) = await RunGeneratorAsync(tempDir.Path);

        Assert.AreEqual(1, exitCode, $"Expected exit code 1. stdout: {stdout}\nstderr: {stderr}");
        StringAssert.Contains(stderr, "Fatal error:");
        StringAssert.Contains(
            stderr,
            "The input does not contain any JSON tokens");
    }

    [TestMethod]
    public async Task EmptyJsonObject_ReturnsSuccessAndExitCode0()
    {
        using var tempDir = new TempDirectory();
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "wmi-classes.json"), "{}");

        var (exitCode, stdout, stderr) = await RunGeneratorAsync(tempDir.Path);

        Assert.AreEqual(0, exitCode, $"Expected exit code 0. stdout: {stdout}\nstderr: {stderr}");
        StringAssert.Contains(stdout, "Classes to generate: 0");
        StringAssert.Contains(stdout, "Generation complete. 0 classes processed.");
    }

    [TestMethod]
    public async Task EmptyClassesArray_ReturnsSuccessAndExitCode0()
    {
        using var tempDir = new TempDirectory();
        await File.WriteAllTextAsync(
            Path.Combine(tempDir.Path, "wmi-classes.json"),
            """{"classes": []}""");

        var (exitCode, stdout, stderr) = await RunGeneratorAsync(tempDir.Path);

        Assert.AreEqual(0, exitCode, $"Expected exit code 0. stdout: {stdout}\nstderr: {stderr}");
        StringAssert.Contains(stdout, "Classes to generate: 0");
        StringAssert.Contains(stdout, "Generation complete. 0 classes processed.");
    }

    [TestMethod]
    public async Task Win32OperatingSystem_GeneratesClassFile()
    {
        using var tempDir = new TempDirectory();
        string outputDir = Path.Combine(tempDir.Path, "Generated");
        string config = """
            {
              "outputDirectory": "Generated",
              "namespace": "TestNamespace",
              "classes": [
                {
                  "wmiNamespace": "root\\cimv2",
                  "wmiClassName": "Win32_OperatingSystem",
                  "className": "OperatingSystem"
                }
              ]
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "wmi-classes.json"), config);

        var (exitCode, stdout, stderr) = await RunGeneratorAsync(tempDir.Path);

        Assert.AreEqual(0, exitCode, $"Expected exit code 0. stdout: {stdout}\nstderr: {stderr}");
        StringAssert.Contains(stdout, "Classes to generate: 1");
        StringAssert.Contains(stdout, "Generation complete. 1 classes processed.");

        string generatedFile = Path.Combine(outputDir, "OperatingSystem.cs");
        Assert.IsTrue(File.Exists(generatedFile), $"Expected generated file at {generatedFile}");

        string content = await File.ReadAllTextAsync(generatedFile);
        StringAssert.Contains(content, "namespace TestNamespace;");
        StringAssert.Contains(content, "public sealed class OperatingSystem");
    }

    [TestMethod]
    public async Task SampleProject_GenerateAndRun()
    {
        string sampleDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "samples",
            "WmiLightClassGenerator.Sample"));

        // Verify generate.cmd exists
        string generateCmd = Path.Combine(sampleDir, "generate.cmd");
        Assert.IsTrue(File.Exists(generateCmd), $"generate.cmd not found at {generateCmd}");

        // Verify generated code exists
        string generatedDir = Path.Combine(sampleDir, "Generated");
        Assert.IsTrue(Directory.Exists(generatedDir), $"Generated directory not found at {generatedDir}");
        Assert.IsTrue(
            File.Exists(Path.Combine(generatedDir, "OperatingSystem.cs")),
            "OperatingSystem.cs not found in Generated directory");
        Assert.IsTrue(
            File.Exists(Path.Combine(generatedDir, "WmiProcess.cs")),
            "WmiProcess.cs not found in Generated directory");

        // Build the sample project
        string sampleCsproj = Path.Combine(sampleDir, "WmiLightClassGenerator.Sample.csproj");
        var (buildExitCode, buildStdout, buildStderr) = await RunProcessAsync(
            "dotnet", $"build \"{sampleCsproj}\" --configuration Debug --no-restore", sampleDir);
        Assert.AreEqual(
            0,
            buildExitCode,
            $"Sample project build failed.\nstdout: {buildStdout}\nstderr: {buildStderr}");

        // Run the sample project and verify output
        var (runExitCode, runStdout, runStderr) = await RunProcessAsync(
            "dotnet", $"run --project \"{sampleCsproj}\" --configuration Debug --no-build", sampleDir);
        Assert.AreEqual(
            0,
            runExitCode,
            $"Sample project run failed.\nstdout: {runStdout}\nstderr: {runStderr}");
        StringAssert.Contains(runStdout, "=== Operating System ===");
        StringAssert.Contains(runStdout, "Caption:");
        StringAssert.Contains(runStdout, "Version:");
        StringAssert.Contains(runStdout, "=== Current Process Owner ===");
        StringAssert.Contains(runStdout, "Owner:");
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunGeneratorAsync(
        string workingDirectory,
        string? configPath = null)
    {
        var args = configPath is not null ? $"\"{configPath}\"" : string.Empty;
        return await RunProcessAsync(ExePath, args, workingDirectory);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start process: {fileName}");

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, stdout, stderr);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            this.Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "WmiLightClassGeneratorTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this.Path))
                {
                    Directory.Delete(this.Path, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup
            }
        }
    }
}
