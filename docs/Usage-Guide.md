# Usage guide

This guide explains how to use the AI agent prompts and instructions in this repository for Xperience by Kentico development.

## Prerequisites

Before you start, you need:

- An AI coding assistant (e.g., [GitHub Copilot](https://github.com/features/copilot), [Cursor](https://cursor.sh/), [Claude Code](https://www.claude.com/product/claude-code))
- Git installed on your machine
- Access to an Xperience by Kentico project

## Install an AI coding assistant

Install an AI coding assistant. This repository contains and focuses on prompts for select popular solutions. To transfer the prompts to other assistants, follow the conventions of your specific solution.

### GitHub Copilot

1. Install the [GitHub Copilot extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) for VS Code.
1. Sign in to your GitHub account.

### Cursor

1. Download and install [Cursor](https://cursor.sh/).
1. Sign in with your account.
1. Configure your AI model preferences.

### Claude Code

1. Install [Claude Code](https://www.claude.com/product/claude-code).
1. Sign in to your Anthropic account.
1. Enable the code editing features.

---

## Installation option A: Plugin marketplace (recommended)

This repository is a plugin marketplace for GitHub Copilot and Claude Code. Plugins let you install commands and MCP servers with a single command, without manually copying files to your project.

> **Note for Cursor:** Cursor does not currently support the shared plugin marketplace format. Use [Option B](#installation-option-b-manual-file-copy) to install files manually.

### Available plugins

| Plugin | Description |
|--------|-------------|
| `kentico-widget-creation` | AI-assisted prompts for creating Page Builder widgets |
| `kentico-kx13-migration` | AI-assisted prompts for migrating KX13 codebase to Xperience by Kentico |

### GitHub Copilot CLI

Install the [GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli), then add the KentiCopilot marketplace and install a plugin:

```bash
# Add the marketplace
copilot plugin marketplace add Kentico/xperience-by-kentico-kenticopilot

# Install a plugin (choose one or both)
copilot plugin install kentico-widget-creation@kentico-kenticopilot
copilot plugin install kentico-kx13-migration@kentico-kenticopilot
```

After installing, the plugin commands are available in any Copilot chat session:

```
/kentico-widget-creation:widget-create-research examples/widget-creation/
/kentico-widget-creation:widget-create-implementation examples/widget-creation/MY_WIDGET.instructions.md
```

### VS Code agent plugins

VS Code also supports browsing and installing agent plugins through the Extensions sidebar:

1. Open the Extensions sidebar in VS Code.
2. Search for `@agentPlugins`.
3. In the **Configure Plugin Marketplaces** section, add this marketplace to `settings.json`:

   ```json
   // .vscode/settings.json
   "chat.plugins.marketplaces": [
       "Kentico/xperience-by-kentico-kenticopilot"
   ]
   ```

4. Browse the KentiCopilot plugins in the Extensions sidebar and click **Install**.

> **Note:** VS Code agent plugins are currently in preview. Enable them with the `chat.plugins.enabled` setting.

### Claude Code

In a Claude Code session, add the marketplace and install a plugin:

```bash
# Add the marketplace
/plugin marketplace add Kentico/xperience-by-kentico-kenticopilot

# Install a plugin (choose one or both)
/plugin install kentico-widget-creation@kentico-kenticopilot
/plugin install kentico-kx13-migration@kentico-kenticopilot
```

After installing, the plugin commands are available as slash commands:

```
/kentico-widget-creation:widget-create-research examples/widget-creation/
/kentico-widget-creation:widget-create-implementation examples/widget-creation/MY_WIDGET.instructions.md
```

> **Note:** MCP servers are configured automatically by the plugin for GitHub Copilot. For Claude Code, add them manually:
>
> **Widget creation:**
> ```bash
> claude mcp add-json kentico.docs.mcp '{"type": "http", "url": "https://docs.kentico.com/mcp"}'
> ```
>
> **KX13 migration (also requires Playwright):**
> ```bash
> claude mcp add-json kentico.docs.mcp '{"type": "http", "url": "https://docs.kentico.com/mcp"}'
> claude mcp add-json playwright-mcp '{"command": "npx", "args": ["@playwright/mcp@latest", "--viewport-size=1920x1080"]}'
> ```

---

## Installation option B: Manual file copy

Clone this repository and copy the files that match your chosen assistant to the root of your project.

### Clone the repository

1. Open your terminal or command prompt.
1. Clone this repository:

   ```bash
   git clone https://github.com/Kentico/xperience-by-kentico-kenticopilot.git
   ```

1. Navigate to the cloned repository:

   ```bash
   cd xperience-by-kentico-kenticopilot
   ```

### Copy files to your project

The repository organizes prompt files by AI assistant. Copy the files that match your chosen assistant to the root of your repository.

#### GitHub Copilot

1. Navigate to the use case directory under `src/`.
2. Copy files from the `gh-copilot/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/gh-copilot/* YOUR_PROJECT/
```

#### Cursor

1. Navigate to the use case directory under `src/`.
2. Copy files from the `cursor/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/cursor/* YOUR_PROJECT/
```

#### Claude Code

1. Navigate to the use case directory under `src/`.
1. Copy files from the `claude-code/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/claude-code/* YOUR_PROJECT/
```

---

## Use the prompts

After installing using either option:

1. Open your project in your AI coding assistant.
1. Open the README file for your specific use case (e.g., `src/widget-creation/README.md`).
1. Follow the instructions in the README.

The README file inside each use case folder explains:

- What the prompts do
- How to trigger them
- What inputs are expected
- Example scenarios
