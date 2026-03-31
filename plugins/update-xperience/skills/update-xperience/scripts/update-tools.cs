#!/usr/bin/env dotnet
#:package System.CommandLine@2.0.0-beta4.22272.1
#:property PublishTrimmed=false

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

// Root command
var rootCommand = new RootCommand("XbK update tool");

// ── detect-versions ────────────────────────────────────────────────────────
var pathArg = new Argument<string?>("projectPath", () => null, "Optional absolute path to project root");
var detectCmd = new Command("detect-versions", "Detect current and latest Xperience package versions");
detectCmd.AddArgument(pathArg);
rootCommand.AddCommand(detectCmd);

detectCmd.SetHandler(async (projectPath) =>
{
    var result = new JsonObject();
    var errors = new JsonArray();

    // 1. Find project root (use provided path, cwd, or search upward)
    string startDir = string.IsNullOrEmpty(projectPath)
        ? Directory.GetCurrentDirectory()
        : Path.GetFullPath(projectPath);
    string? projectRoot = FindProjectRoot(startDir);
    result["projectRoot"] = projectRoot ?? Directory.GetCurrentDirectory();

    // 2. Read NuGet version
    string? nugetVersion = null;
    string? nugetSource = null;

    if (projectRoot != null)
    {
        // Prefer Directory.Packages.props (central package management)
        string dpp = Path.Combine(projectRoot, "Directory.Packages.props");
        if (File.Exists(dpp))
        {
            nugetVersion = ReadVersionFromProps(dpp, "Kentico.Xperience");
            if (nugetVersion != null)
            {
                nugetSource = dpp;
            }
        }

        // Fall back to any .csproj in the tree
        if (nugetVersion == null)
        {
            foreach (string csproj in Directory.EnumerateFiles(projectRoot, "*.csproj", SearchOption.AllDirectories))
            {
                nugetVersion = ReadVersionFromCsproj(csproj, "Kentico.Xperience");
                if (nugetVersion != null)
                { nugetSource = csproj; break; }
            }
        }
    }

    result["nuget"] = nugetVersion;
    result["nugetSource"] = nugetSource;

    // 3. Read npm version from package.json
    string? npmVersion = null;
    string? npmSource = null;
    string? npmPackageName = null;

    if (projectRoot != null)
    {
        foreach (string? pkgJson in Directory.EnumerateFiles(projectRoot, "package.json", SearchOption.AllDirectories)
                     .Where(p => !p.Contains("node_modules")))
        {
            (npmVersion, npmPackageName) = ReadVersionFromPackageJson(pkgJson, "@kentico/xperience");
            if (npmVersion != null)
            { npmSource = pkgJson; break; }
        }
    }

    result["npm"] = npmVersion;
    result["npmPackageName"] = npmPackageName;
    result["npmSource"] = npmSource;

    // 4. Query NuGet for latest via dotnet package search
    string? latestVersion = null;
    if (nugetVersion != null)
    {
        (latestVersion, string? searchError) = await QueryLatestNuGetVersion("Kentico.Xperience.Core");
        if (searchError != null)
        {
            errors.Add(searchError);
        }
    }
    result["latest"] = latestVersion;

    // 5. Compute update available
    result["updateAvailable"] = nugetVersion != null && latestVersion != null && nugetVersion != latestVersion;

    if (errors.Count > 0)
    {
        result["errors"] = errors;
    }

    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));

    // Exit 1 if we couldn't detect the current version at all
    int exitCode = (nugetVersion == null && npmVersion == null) ? 1 : 0;
    Environment.Exit(exitCode);
}, pathArg);

return await rootCommand.InvokeAsync(args);

// ── Helpers ────────────────────────────────────────────────────────────────

static string? FindProjectRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        if (dir.EnumerateFiles("*.csproj").Any() ||
            dir.EnumerateFiles("Directory.Packages.props").Any() ||
            dir.EnumerateFiles("*.sln").Any())
        {
            return dir.FullName;
        }

        dir = dir.Parent;
    }
    return null;
}

static string? ReadVersionFromProps(string filePath, string packagePrefix)
{
    try
    {
        var doc = XDocument.Load(filePath);
        string? version = doc.Descendants("PackageVersion")
            .Where(e => e.Attribute("Include")?.Value.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase) == true)
            .Select(e => e.Attribute("Version")?.Value)
            .FirstOrDefault(v => v != null);
        return version;
    }
    catch { return null; }
}

static string? ReadVersionFromCsproj(string filePath, string packagePrefix)
{
    try
    {
        var doc = XDocument.Load(filePath);
        string? version = doc.Descendants("PackageReference")
            .Where(e => e.Attribute("Include")?.Value.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase) == true)
            .Select(e => e.Attribute("Version")?.Value ?? e.Element("Version")?.Value)
            .FirstOrDefault(v => v != null);
        return version;
    }
    catch { return null; }
}

static (string? version, string? packageName) ReadVersionFromPackageJson(string filePath, string packagePrefix)
{
    try
    {
        var json = JsonNode.Parse(File.ReadAllText(filePath));
        foreach (string? section in new[] { "dependencies", "devDependencies" })
        {
            var deps = json?[section]?.AsObject();
            if (deps == null)
            {
                continue;
            }

            foreach (var (name, value) in deps)
            {
                if (name.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string raw = value?.GetValue<string>() ?? "";
                    // Strip leading ^ ~ >= etc.
                    string clean = Regex.Replace(raw, @"^[^0-9]*", "");
                    return (clean.Length > 0 ? clean : raw, name);
                }
            }
        }
    }
    catch { }
    return (null, null);
}

static async Task<(string? version, string? error)> QueryLatestNuGetVersion(string packageId)
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", $"package search {packageId} --exact-match --format json")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi)!;
        string stdout = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            return (null, $"dotnet package search exited {proc.ExitCode}");
        }

        // Output schema: { "searchResult": [ { "packages": [ { "id": "...", "version": "..." } ] } ] }
        // Get the last package in the first result (latest version)
        var doc = JsonNode.Parse(stdout);
        var packages = doc?["searchResult"]?[0]?["packages"]?.AsArray();
        string? version = packages?.Count > 0
            ? packages[packages.Count - 1]?["version"]?.GetValue<string>()
            : null;
        return (version, version == null ? "Could not parse dotnet package search output" : null);
    }
    catch (Exception ex)
    {
        return (null, $"dotnet package search failed: {ex.Message}");
    }
}

#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
