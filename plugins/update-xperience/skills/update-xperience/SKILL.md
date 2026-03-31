---
name: update-xperience
description: Update Xperience by Kentico packages safely. Detect current+latest versions with scripts/update-tools.cs, check changelog breaking changes, bump NuGet/npm versions, validate build/tests, and update documented version references. Use when user asks to update, upgrade, move to latest version, run version audit, or review breaking changes.
compatibility: Requires .NET SDK (dotnet), Node.js/npm, internet access for package/changelog lookup, and a repo containing an Xperience by Kentico project.
allowed-tools: Read Edit Bash(dotnet:*) Bash(npm:*) Bash(git:*) WebFetch
---

# Update Xperience Project

Complete all steps below autonomously unless the user prompts otherwise.

## Quick start

```
1. Detect current versions via update-tools app
2. Find latest versions available via kentico-docs MCP - Changelog
3. Check for breaking changes or deprecated APIs between current and new versions
4. Update .csproj/Directory.Packages.props and package.json
5. Run dotnet/npm install
6. Validate compilation
7. Run `dotnet run -- --kxp-update --skip-confirmation` to update Xperience database without user intervention
8. Update version refs in docs/config
```

## Full Workflow

### Phase 1: Discovery

**Step 1: Detect current versions**

- Run the update-tools app from the skill folder:
  - `dotnet run ./scripts/update-tools.cs detect-versions <project-root>`
  - This script is found relative to the current skill file (`./SKILL.md`) and is not in the Xperience application repository
- Use the JSON output fields as source of truth:
  - `nuget`, `nugetSource` for current .NET package version and where it was detected
  - `npm`, `npmPackageName`, `npmSource` for current npm package version and where it was detected
  - `latest` for latest Xperience NuGet version (`Kentico.Xperience.Core`)
  - `updateAvailable` to quickly determine if upgrade work is needed
- If `errors` is present, report them and stop before Phase 2

**Step 2: Check breaking changes**

- Use `mcp_kentico-docs_kentico_docs_search` to query for Changelog details for the version updates
- Identify API removals, renamed methods, deprecated features
- Note migration steps required
- List any new required dependencies

### Phase 2: Update

- Ensure the application is not running and there are no uncommitted changes before proceeding with updates
- `dotnet run ./scripts/update-tools.cs check-preconditions <project-root>`
  - This script is found relative to the current skill file (`./SKILL.md`) and is not in the Xperience application repository
- If these requirements are not met, stop and report to the user

**Steps 3–5: Update NuGet packages, npm packages, and version references**

Run the update-versions command to handle all three steps deterministically:

```
dotnet run ./scripts/update-tools.cs update-versions <project-root> --from <current-version> --to <new-version>
```

- Updates `Directory.Packages.props` or `.csproj` files for all `Kentico.Xperience.*` packages
- Updates `package.json` for all `@kentico/xperience-*` packages (preserves `^`/`~` range specifiers)
- Replaces plain-text version references in `README.md`, `.github/workflows/`, and `mcp.json`
- Returns a JSON result with `updated` (list of modified files and replacement counts) and `errors`
- If `errors` is present, report them and stop before running restore/install

After a successful run:

- Run: `dotnet restore`
- Run: `npm install` (if npm packages were updated)
- Check `package-lock.json` was updated
- If npm dependency resolution fails with non-trivial conflicts (peer deps, Node engine mismatch, transitive version deadlock), stop and report exact errors plus impacted packages

### Phase 3: Validation

**Step 6: Compile & validate**

- Run: `dotnet build` (full rebuild, not just restore)
- Run: `npm run build` (if build script exists)
- Capture any compilation errors
- Check for deprecated API warnings

**Step 7: Verify breaking changes**

- Search codebase for removed/renamed APIs from breaking changes list
- Run code analysis or linter if available

**Step 8: Run automated tests**

- Run: `dotnet test` (unit/integration tests)
- Run: `npm test` (if test script exists)
- Follow documentation for custom test infrastructure (e.g. E2E tests)
- Verify all tests pass without errors

**Step 9: Document**

- Create summary of changes by category:
  - ✅ NuGet packages updated (list versions)
  - ✅ npm packages updated (list versions)
  - ⚠️ Breaking changes handled (list mitigations)
  - 📝 Files modified (list touched files)
- Suggest next steps (resolve API deprecations or removal, leverage new product features, implement recommended best practices)

## Advanced Features

### Handling Breaking Changes

When breaking changes detected:

1. **Auto-fixable** (renamed methods): Use search-replace or refactoring tools
2. **Manual review** (API removals): Flag for human decision, don't auto-remove
3. **Requires migration:** Link to kentico-docs for detailed migration guide

If breaking changes cannot be resolved:

1. Stop the update process
2. Report which APIs have changed
3. Link to the relevant Xperience by Kentico documentation, if available

### Dependency Conflict Policy

- Handle only straightforward version bumps that install cleanly
- If dependency issues are complex, do not attempt manual pinning chains or broad package churn
- Stop and report:
  - failed command output
  - conflicting packages and requested ranges
  - suggested next action for human review

## Use Cases

- **Routine update**: "Update Xperience to latest" → executes full workflow
- **Major version jump**: "Update from 30.12.0 to 31.3.0" → emphasizes breaking changes phase
- **Post-release review**: "Run update validation" → jumps to Phase 3
- **Version audit**: "What versions are we on?" → runs only Phase 1 discovery

## When to Use This Skill

✅ Updating Xperience packages for security patches, features, or bug fixes  
✅ Planning major version upgrades with breaking changes  
✅ Onboarding new team members (show current versions)

❌ Not for: Updating non-Xperience dependencies
❌ Not for: Downgrading versions
