# KentiCopilot: KX13 Codebase Migration Plugin

AI-assisted prompts for migrating the **codebase** of Kentico Xperience 13 projects to [Xperience by Kentico](https://docs.kentico.com/x/migrate_from_kx13_guides).

## Installation

### GitHub Copilot (VS Code) and Claude Code

Add the KentiCopilot marketplace to your CLI, then install this plugin:

```bash
# GitHub Copilot CLI
copilot plugin marketplace add Kentico/xperience-by-kentico-kenticopilot
copilot plugin install kentico-kx13-migration@kentico-kenticopilot

# Claude Code
/plugin marketplace add Kentico/xperience-by-kentico-kenticopilot
/plugin install kentico-kx13-migration@kentico-kenticopilot
```

### Cursor

Cursor does not currently support the shared plugin marketplace format. Clone the repository and copy the Cursor-specific files to your project:

```bash
git clone https://github.com/Kentico/xperience-by-kentico-kenticopilot.git
cp -r xperience-by-kentico-kenticopilot/src/kx13-codebase-migration/cursor/* YOUR_PROJECT/
```

## Commands

| Command | Description |
|---------|-------------|
| `/kentico-kx13-migration:migrate-global-code` | Migrate global code, generate code files, and set up the project foundation |
| `/kentico-kx13-migration:migrate-shared-component` | Migrate reusable components (headers, footers, navigation elements) |
| `/kentico-kx13-migration:migrate-page-widgets` | Migrate Page Builder widgets and sections used by a specific page |
| `/kentico-kx13-migration:migrate-page` | Migrate a page and its business/presentation logic |
| `/kentico-kx13-migration:migrate-page-visual` | Ensure a migrated page visually matches its original KX13 page |

## Included MCP Servers

This plugin automatically configures the following MCP servers:

| Server | Description |
|--------|-------------|
| `kentico.docs.mcp` | [Xperience by Kentico Documentation MCP server](https://docs.kentico.com/x/mcp_server_xp) |
| `playwright-mcp` | [Playwright MCP server](https://github.com/microsoft/playwright-mcp) for visual comparison |

> **Note for Claude Code:** MCP servers configured in `.mcp.json` plugin files are not yet supported. Add the MCP servers manually:
>
> ```bash
> claude mcp add-json kentico.docs.mcp '{"type": "http", "url": "https://docs.kentico.com/mcp"}'
> claude mcp add-json playwright-mcp '{"command": "npx", "args": ["@playwright/mcp@latest", "--viewport-size=1920x1080"]}'
> ```

## Workflow

Use the commands in the following sequence:

### 1. Global code migration

```
/kentico-kx13-migration:migrate-global-code
```

### 2. Page migration (per page)

```
/kentico-kx13-migration:migrate-page-widgets
pageName: home
legacyPageUrl: https://localhost:5001/en-us/home

/kentico-kx13-migration:migrate-page
pageName: home
legacyPageUrl: https://localhost:5001/en-us/home
```

### 3. Shared components

```
/kentico-kx13-migration:migrate-shared-component
componentName: breadcrumbs
legacyPageUrl: https://localhost:5001/en-us/home
```

### 4. Visual verification (when needed)

```
/kentico-kx13-migration:migrate-page-visual
pageName: home
legacyPageUrl: https://localhost:5001/en-us/home
newPageUrl: http://localhost:60444/en-us/home
```

## Prerequisites

Before using this plugin:

1. Have a Kentico Xperience 13 project (source) running locally.
2. Have an Xperience by Kentico project (target) connected to a database migrated using the [Kentico Migration Tool](https://github.com/Kentico/xperience-by-kentico-kentico-migration-tool).
3. Place both projects in the same workspace:
   ```
   KX13/    # Kentico Xperience 13 project files
   XbyK/    # Xperience by Kentico project files
   ```

## More information

See the [KX13 migration README](../../src/kx13-codebase-migration/README.md) for detailed usage instructions.
