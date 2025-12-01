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

The repository organizes prompt files by AI assistant. Copy the files that match your chosen assistant to the root of your repository.

### GitHub Copilot

1. Navigate to the use case directory under `src/`.
2. Copy files from the `gh-copilot/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/gh-copilot/* YOUR_PROJECT/
```

### Cursor

1. Navigate to the use case directory under `src/`.
2. Copy files from the `cursor/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/cursor/* YOUR_PROJECT/
```

### Claude Code

1. Navigate to the use case directory under `src/`.
1. Copy files from the `claude-code/` subdirectory to the root of your project.

**Example for widget creation:**

```bash
cp -r src/widget-creation/claude-code/* YOUR_PROJECT/
```

## Use the prompts

After copying the files to your project:

1. Open your project in your AI coding assistant.
1. Open the README file for your specific use case (e.g., `src/widget-creation/README.md`).
1. Follow the instructions in the README.

The README file inside each use case folder explains:

- What the prompts do
- How to trigger them
- What inputs are expected
- Example scenarios
