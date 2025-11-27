# Usage guide

This guide explains how to use the AI agent prompts and instructions in this repository for Xperience by Kentico development.

## Prerequisites

Before you start, you need:

- An AI coding assistant installed ([GitHub Copilot](https://github.com/features/copilot), [Cursor](https://cursor.sh/), or [Claude Code](https://claude.ai/))
- Git installed on your machine
- Access to an Xperience by Kentico project (for testing prompts)

## Install AI coding assistant

Choose and install one of the supported AI coding assistants:

### GitHub Copilot

1. Install the [GitHub Copilot extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) for VS Code.
1. Sign in to your GitHub account.
1. Activate your GitHub Copilot subscription.

### Cursor

1. Download and install [Cursor](https://cursor.sh/).
1. Sign in with your account.
1. Configure your AI model preferences.

### Claude Code

1. Install [Claude Code](https://github.com/anthropics/claude-code).
1. Sign in to your Anthropic account.
1. Enable the code editing features.

## Clone the repository

1. Open your terminal or command prompt.
1. Clone this repository:

   ```bash
   git clone https://github.com/Kentico/xperience-by-kentico-kenticopilot.git
   ```

1. Navigate to the cloned repository:

   ```bash
   cd xperience-by-kentico-kenticopilot
   ```

## Copy files to your project

The repository organizes prompt files by AI assistant. Copy the files that match your chosen assistant.

### GitHub Copilot

1. Navigate to the use case directory under `src/`.
1. Copy files from the `gh-copilot/` subdirectory.
1. Paste them into your project's `.github/` directory.

**Example for widget creation:**

```bash
cp -r src/widget-creation/gh-copilot/.github/* YOUR_PROJECT/.github/
```

### Cursor

1. Navigate to the use case directory under `src/`.
1. Copy files from the `cursor/` subdirectory.
1. Paste them into your project's `.cursor/` directory.

**Example for widget creation:**

```bash
cp -r src/widget-creation/cursor/.cursor/* YOUR_PROJECT/.cursor/
```

### Claude Code

1. Navigate to the use case directory under `src/`.
1. Copy files from the `claude-code/` subdirectory.
1. Paste them into your project's `.claude/` directory.

**Example for widget creation:**

```bash
cp -r src/widget-creation/claude-code/.claude/* YOUR_PROJECT/.claude/
```

## Use the prompts

After copying the files to your project:

1. Open your project in your AI coding assistant.
1. Open the README file for your specific use case (e.g., `src/widget-creation/README.md`).
1. Follow the instructions in the README.

Each use case README explains:

- What the prompts do
- How to trigger them
- What inputs they expect
- Example scenarios
