# KentiCopilot: Widget Creation Plugin

AI-assisted prompts for creating [Page Builder](https://docs.kentico.com/x/6QWiCQ) widgets in Xperience by Kentico.

## Installation

### GitHub Copilot (VS Code) and Claude Code

Add the KentiCopilot marketplace to your CLI, then install this plugin:

```bash
# GitHub Copilot CLI
copilot plugin marketplace add Kentico/xperience-by-kentico-kenticopilot
copilot plugin install kentico-widget-creation@kentico-kenticopilot

# Claude Code
/plugin marketplace add Kentico/xperience-by-kentico-kenticopilot
/plugin install kentico-widget-creation@kentico-kenticopilot
```

### Cursor

Cursor does not currently support the shared plugin marketplace format. Clone the repository and copy the Cursor-specific files to your project:

```bash
git clone https://github.com/Kentico/xperience-by-kentico-kenticopilot.git
cp -r xperience-by-kentico-kenticopilot/src/widget-creation/cursor/* YOUR_PROJECT/
```

## Commands

| Command | Description |
|---------|-------------|
| `/kentico-widget-creation:widget-create-research` | Analyze widget requirements and generate detailed implementation instructions |
| `/kentico-widget-creation:widget-create-implementation` | Create a Page Builder widget following the generated instructions |

## Included MCP Servers

This plugin automatically configures the [Xperience by Kentico Documentation MCP server](https://docs.kentico.com/x/mcp_server_xp):

| Server | Description |
|--------|-------------|
| `kentico.docs.mcp` | Provides AI access to the full Xperience by Kentico documentation |

## Workflow

The widget creation workflow uses two commands in sequence:

### 1. Research stage

Analyzes your requirements and design, validates them against Xperience documentation, and generates detailed implementation instructions.

```
/kentico-widget-creation:widget-create-research examples/widget-creation/
```

### 2. Implementation stage

Creates the widget code following the generated instructions, project conventions, and Xperience best practices.

```
/kentico-widget-creation:widget-create-implementation examples/widget-creation/ARTICLE_SHOWCASE.instructions.md
```

## Prerequisites

Before using this plugin:

1. Have an Xperience by Kentico project with Page Builder configured.
2. Create a folder with your widget requirements:
   - `requirements.md` - Describes widget functionality and technical requirements
   - `design.html` (optional) - Visual design exported from Figma or similar tool

See the [`examples/widget-creation/`](../../examples/widget-creation/) directory for sample files.

## What the plugin generates

The implementation command produces:

- Widget view component class
- Widget properties class
- Razor view file (`.cshtml`)
- View model class
- Localized resource strings

## More information

See the [widget creation README](../../src/widget-creation/README.md) for detailed usage instructions.
