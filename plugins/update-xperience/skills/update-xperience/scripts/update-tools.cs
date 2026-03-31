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
var pathArg = new Argument<string>("projectPath", "Absolute path to project root");
var detectCmd = new Command("detect-versions", "Detect current and latest Xperience package versions");
detectCmd.AddArgument(pathArg);
rootCommand.AddCommand(detectCmd);

detectCmd.SetHandler(async (projectPath) =>
{
    var result = new JsonObject();
    var errors = new JsonArray();

    // 1. Find project root
    string startDir = Path.GetFullPath(projectPath);
    if (!Directory.Exists(startDir))
    {
        errors.Add($"projectPath '{startDir}' does not exist.");
        result["errors"] = errors;
        Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
        Environment.Exit(1);
        return;
    }
    string? projectRoot = FindProjectRoot(startDir);
    result["projectRoot"] = projectRoot ?? startDir;

    // 2. Read NuGet version
    string? nugetVersion = null;
    string? nugetSource = null;

    if (projectRoot != null)
    {
        // Prefer Directory.Packages.props (central package management)
        var dpp = Path.Combine(projectRoot, "Directory.Packages.props");
        if (File.Exists(dpp))
        {
            nugetVersion = ReadVersionFromProps(dpp, "Kentico.Xperience");
            if (nugetVersion != null) nugetSource = dpp;
        }

        // Fall back to any .csproj in the tree
        if (nugetVersion == null)
        {
            foreach (var csproj in Directory.EnumerateFiles(projectRoot, "*.csproj", SearchOption.AllDirectories))
            {
                nugetVersion = ReadVersionFromCsproj(csproj, "Kentico.Xperience");
                if (nugetVersion != null) { nugetSource = csproj; break; }
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
        foreach (var pkgJson in Directory.EnumerateFiles(projectRoot, "package.json", SearchOption.AllDirectories)
                     .Where(p => !p.Contains("node_modules")))
        {
            (npmVersion, npmPackageName) = ReadVersionFromPackageJson(pkgJson, "@kentico/xperience");
            if (npmVersion != null) { npmSource = pkgJson; break; }
        }
    }

    result["npm"] = npmVersion;
    result["npmPackageName"] = npmPackageName;
    result["npmSource"] = npmSource;

    // 4. Query NuGet for latest via dotnet package search
    // Kentico.Xperience.Core is always a stable release; other packages may carry a -preview suffix
    // at the same version number, but the canonical version to compare against is always the stable core.
    string? latestVersion = null;
    if (nugetVersion != null)
    {
        (latestVersion, var searchError) = await QueryLatestNuGetVersion("Kentico.Xperience.Core");
        if (searchError != null) errors.Add(searchError);
    }
    result["latest"] = latestVersion;

    // 5. Compute update available — strip any -preview suffix from current version before comparing
    var nugetBase = nugetVersion?.Split('-')[0];
    result["updateAvailable"] = nugetBase != null && latestVersion != null && nugetBase != latestVersion;

    if (errors.Count > 0) result["errors"] = errors;

    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));

    // Exit 1 if we couldn't detect the current version at all
    var exitCode = (nugetVersion == null && npmVersion == null) ? 1 : 0;
    Environment.Exit(exitCode);
}, pathArg);

// ── check-preconditions ───────────────────────────────────────────────────
var preconPathArg = new Argument<string>("projectPath", "Absolute path to project root");
var preconCmd = new Command("check-preconditions", "Verify app is stopped and git tree is clean before updating");
preconCmd.AddArgument(preconPathArg);
rootCommand.AddCommand(preconCmd);

preconCmd.SetHandler((projectPath) =>
{
    var result = new JsonObject();
    var failures = new JsonArray();

    string startDir = Path.GetFullPath(projectPath);
    if (!Directory.Exists(startDir))
    {
        failures.Add($"projectPath '{startDir}' does not exist.");
        result["passed"] = false;
        result["failures"] = failures;
        Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
        Environment.Exit(1);
        return;
    }
    string? projectRoot = FindProjectRoot(startDir);
    result["projectRoot"] = projectRoot ?? startDir;

    // 1. Git clean check
    var gitStatus = RunProcess("git", "status --porcelain", projectRoot ?? startDir);
    if (gitStatus.exitCode != 0)
    {
        failures.Add("git status failed — is this a git repository?");
    }
    else if (!string.IsNullOrWhiteSpace(gitStatus.stdout))
    {
        var lines = gitStatus.stdout.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        failures.Add($"Uncommitted changes detected ({lines.Length} file(s)). Commit or stash before updating.");
        result["uncommittedFiles"] = new JsonArray(lines.Select(l => (JsonNode)JsonValue.Create(l.Trim())!).ToArray());
    }

    // 2. Kestrel / dotnet run process check
    // Look for dotnet processes with a workdir under the project root
    if (projectRoot != null)
    {
        var runningPids = new JsonArray();
        try
        {
            // Use `dotnet-trace` isn't available everywhere; use ps/tasklist instead
            bool isWindows = OperatingSystem.IsWindows();
            ProcessResult psResult = isWindows
                ? RunProcess("tasklist", "/FI \"IMAGENAME eq dotnet.exe\" /FO CSV /NH", null)
                : RunProcess("pgrep", "-a dotnet", null);

            if (psResult.exitCode == 0 && !string.IsNullOrWhiteSpace(psResult.stdout))
            {
                // Heuristic: any dotnet process whose cmdline contains the project root path
                var lines = psResult.stdout.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains(projectRoot, StringComparison.OrdinalIgnoreCase))
                        runningPids.Add(line.Trim());
                }
            }
        }
        catch { /* process enumeration is best-effort */ }

        if (runningPids.Count > 0)
        {
            failures.Add($"Application appears to be running ({runningPids.Count} dotnet process(es) referencing the project root). Stop the app before updating.");
            result["runningProcesses"] = runningPids;
        }
    }

    result["passed"] = failures.Count == 0;
    if (failures.Count > 0)
        result["failures"] = failures;

    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    Environment.Exit(failures.Count == 0 ? 0 : 1);
}, preconPathArg);

// ── update-versions ───────────────────────────────────────────────────────
var updatePathArg = new Argument<string>("projectPath", "Absolute path to project root");
var fromOpt = new Option<string>("--from", "Current version to replace") { IsRequired = true };
var toOpt = new Option<string>("--to", "New version to set") { IsRequired = true };
var updateCmd = new Command("update-versions", "Update Xperience package versions in NuGet, npm, and doc files");
updateCmd.AddArgument(updatePathArg);
updateCmd.AddOption(fromOpt);
updateCmd.AddOption(toOpt);
rootCommand.AddCommand(updateCmd);

updateCmd.SetHandler((projectPath, fromVersion, toVersion) =>
{
    var result = new JsonObject();
    var errors = new JsonArray();
    var updated = new JsonArray();

    string startDir = Path.GetFullPath(projectPath);
    if (!Directory.Exists(startDir))
    {
        errors.Add($"projectPath '{startDir}' does not exist.");
        result["errors"] = errors;
        Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
        Environment.Exit(1);
        return;
    }

    string? projectRoot = FindProjectRoot(startDir);
    result["projectRoot"] = projectRoot ?? startDir;

    if (projectRoot == null)
    {
        errors.Add("Could not find project root (no .csproj, .sln, or Directory.Packages.props found).");
        result["errors"] = errors;
        Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
        Environment.Exit(1);
        return;
    }

    // Step 3: Update NuGet versions
    // Prefer Directory.Packages.props
    var dpp = Path.Combine(projectRoot, "Directory.Packages.props");
    if (File.Exists(dpp))
    {
        if (ReplaceVersionInXml(dpp, "PackageVersion", "Version", "Kentico.Xperience", fromVersion, toVersion, out var count))
        {
            updated.Add(new JsonObject { ["file"] = dpp, ["type"] = "nuget-props", ["replacements"] = count });
        }
        else
        {
            errors.Add($"Directory.Packages.props found but no Kentico.Xperience version '{fromVersion}' matched.");
        }
    }
    else
    {
        // Fall back to .csproj files
        foreach (var csproj in Directory.EnumerateFiles(projectRoot, "*.csproj", SearchOption.AllDirectories))
        {
            if (ReplaceVersionInXml(csproj, "PackageReference", "Version", "Kentico.Xperience", fromVersion, toVersion, out var count))
                updated.Add(new JsonObject { ["file"] = csproj, ["type"] = "nuget-csproj", ["replacements"] = count });
        }
        if (updated.Count == 0)
            errors.Add($"No .csproj files contained a Kentico.Xperience PackageReference with version '{fromVersion}'.");
    }

    // Step 4: Update npm versions
    foreach (var pkgJson in Directory.EnumerateFiles(projectRoot, "package.json", SearchOption.AllDirectories)
                 .Where(p => !p.Contains("node_modules")))
    {
        if (ReplaceVersionInPackageJson(pkgJson, "@kentico/xperience", fromVersion, toVersion, out var count))
            updated.Add(new JsonObject { ["file"] = pkgJson, ["type"] = "npm", ["replacements"] = count });
    }

    // Step 5: Update hardcoded version references in docs and CI
    var docPatterns = new[] { "README.md", "*.md" };
    var ciGlobs = new[] { ".github/workflows/*.yml", ".github/workflows/*.yaml" };
    var miscFiles = new[] { "mcp.json" };

    var candidateFiles = new List<string>();
    foreach (var pattern in docPatterns)
        candidateFiles.AddRange(Directory.EnumerateFiles(projectRoot, pattern, SearchOption.AllDirectories)
            .Where(p => !p.Contains("node_modules")));
    foreach (var glob in ciGlobs)
    {
        var dir = Path.Combine(projectRoot, Path.GetDirectoryName(glob)!);
        if (Directory.Exists(dir))
            candidateFiles.AddRange(Directory.EnumerateFiles(dir, Path.GetFileName(glob)));
    }
    foreach (var name in miscFiles)
    {
        var f = Path.Combine(projectRoot, name);
        if (File.Exists(f)) candidateFiles.Add(f);
    }

    foreach (var file in candidateFiles.Distinct())
    {
        if (ReplaceVersionInTextFile(file, fromVersion, toVersion, out var count))
            updated.Add(new JsonObject { ["file"] = file, ["type"] = "text", ["replacements"] = count });
    }

    result["fromVersion"] = fromVersion;
    result["toVersion"] = toVersion;
    result["updated"] = updated;
    if (errors.Count > 0) result["errors"] = errors;

    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    Environment.Exit(errors.Count == 0 ? 0 : 1);
}, updatePathArg, fromOpt, toOpt);

return await rootCommand.InvokeAsync(args);

// ── Helpers ────────────────────────────────────────────────────────────────

static ProcessResult RunProcess(string exe, string arguments, string? workingDir)
{
    try
    {
        var psi = new ProcessStartInfo(exe, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory()
        };
        using var proc = Process.Start(psi)!;
        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return new ProcessResult(proc.ExitCode, stdout, stderr);
    }
    catch (Exception ex)
    {
        return new ProcessResult(-1, "", ex.Message);
    }
}

static string? FindProjectRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        if (dir.EnumerateFiles("*.csproj").Any() ||
            dir.EnumerateFiles("Directory.Packages.props").Any() ||
            dir.EnumerateFiles("*.sln").Any())
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

static string? ReadVersionFromProps(string filePath, string packagePrefix)
{
    try
    {
        var doc = XDocument.Load(filePath);
        var version = doc.Descendants("PackageVersion")
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
        var version = doc.Descendants("PackageReference")
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
        foreach (var section in new[] { "dependencies", "devDependencies" })
        {
            var deps = json?[section]?.AsObject();
            if (deps == null) continue;
            foreach (var (name, value) in deps)
            {
                if (name.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var raw = value?.GetValue<string>() ?? "";
                    // Strip leading ^ ~ >= etc.
                    var clean = Regex.Replace(raw, @"^[^0-9]*", "");
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
        var stdout = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0) return (null, $"dotnet package search exited {proc.ExitCode}");

        // Output schema: { "searchResult": [ { "packages": [ { "latestVersion": "..." } ] } ] }
        var doc = JsonNode.Parse(stdout);
        var version = doc?["searchResult"]?[0]?["packages"]?[0]?["latestVersion"]?.GetValue<string>();
        return (version, version == null ? "Could not parse dotnet package search output" : null);
    }
    catch (Exception ex)
    {
        return (null, $"dotnet package search failed: {ex.Message}");
    }
}

// Replaces Version attribute/element on matching PackageReference or PackageVersion elements in an XML file.
static bool ReplaceVersionInXml(string filePath, string elementName, string versionAttrOrElem,
    string packagePrefix, string fromVersion, string toVersion, out int replacements)
{
    replacements = 0;
    try
    {
        var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
        foreach (var el in doc.Descendants(elementName))
        {
            var include = el.Attribute("Include")?.Value ?? "";
            if (!include.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase)) continue;

            var attr = el.Attribute(versionAttrOrElem);
            if (attr != null && attr.Value == fromVersion)
            {
                attr.Value = toVersion;
                replacements++;
            }
            else
            {
                var elem = el.Element(versionAttrOrElem);
                if (elem != null && elem.Value == fromVersion)
                {
                    elem.Value = toVersion;
                    replacements++;
                }
            }
        }
        if (replacements > 0)
            doc.Save(filePath);
        return replacements > 0;
    }
    catch { return false; }
}

// Replaces package versions in package.json dependencies/devDependencies for packages matching prefix.
static bool ReplaceVersionInPackageJson(string filePath, string packagePrefix,
    string fromVersion, string toVersion, out int replacements)
{
    replacements = 0;
    try
    {
        var text = File.ReadAllText(filePath);
        var json = JsonNode.Parse(text)?.AsObject();
        if (json == null) return false;

        bool changed = false;
        foreach (var section in new[] { "dependencies", "devDependencies" })
        {
            var deps = json[section]?.AsObject();
            if (deps == null) continue;
            foreach (var key in deps.Select(kv => kv.Key).ToList())
            {
                if (!key.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase)) continue;
                var raw = deps[key]?.GetValue<string>() ?? "";
                // Preserve leading range specifier (^, ~, >=, etc.)
                var rangePrefix = Regex.Match(raw, @"^[^0-9]*").Value;
                var clean = raw[rangePrefix.Length..];
                if (clean == fromVersion)
                {
                    deps[key] = rangePrefix + toVersion;
                    replacements++;
                    changed = true;
                }
            }
        }

        if (changed)
            File.WriteAllText(filePath, JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
        return changed;
    }
    catch { return false; }
}

// Replaces all plain-text occurrences of fromVersion with toVersion in a file.
static bool ReplaceVersionInTextFile(string filePath, string fromVersion, string toVersion, out int replacements)
{
    replacements = 0;
    try
    {
        var text = File.ReadAllText(filePath);
        replacements = Regex.Count(text, Regex.Escape(fromVersion));
        if (replacements > 0)
            File.WriteAllText(filePath, text.Replace(fromVersion, toVersion, StringComparison.Ordinal));
        return replacements > 0;
    }
    catch { return false; }
}

record ProcessResult(int exitCode, string stdout, string stderr);

#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
