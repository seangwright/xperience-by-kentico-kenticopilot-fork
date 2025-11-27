# Contributing setup

This repository contains AI agent prompts, instructions, and related materials for Xperience by Kentico development assistance. This guide explains how to contribute changes to these materials.

## Required software

### Text editor

You need a text editor to work with Markdown and prompt files. We recommend [VS Code](https://code.visualstudio.com/) for its Markdown support and helpful extensions.

### AI coding assistants

You can test your prompt changes with AI assistants, but this is optional. This repository currently supports:

- [GitHub Copilot](https://github.com/features/copilot)
- [Cursor](https://cursor.sh/)
- [Claude Code](https://claude.ai/)

Testing with these tools helps validate that your prompt changes work as intended.

## Repository structure

- `src/` - Prompt files organized by use case (e.g., `widget-creation/`)
- `examples/` - Examples of files passed to LLMs as context for corresponding scenarios

### AI assistant organization

Prompt files are organized by AI assistant:

- `claude-code/` - Claude Code configurations
- `cursor/` - Cursor configurations
- `gh-copilot/` - GitHub Copilot configurations

## Contributing to prompt files

### Prompt engineering best practices

When you create or modify prompt files:

- Write clear, specific instructions
- Include context and examples
- Test prompts with the target AI assistant
- Follow the structure of existing prompts

### Multi-assistant support

When you contribute prompts for multiple AI assistants:

- Organize files in assistant-specific subdirectories (`claude-code/`, `cursor/`, `gh-copilot/`)
- Follow each assistant's configuration conventions
- Test with the target assistant

## Development workflow

1. Create a new branch with one of the following prefixes:

   - `feat/` - new functionality
   - `refactor/` - restructuring of existing features
   - `fix/` - bugfixes

1. Validate your Markdown formatting:

   - Use VS Code's Markdown preview to check formatting
   - Verify proper syntax and link validity
   - Follow existing file organization patterns

1. Commit your changes with a commit message following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

   - See [`.github/instructions/commit-messages.instructions.md`](.github/instructions/commit-messages.instructions.md) for guidelines

1. Create a pull request on GitHub.

   - Write a clear description of the changes
   - Include screenshots or recordings demonstrating prompt testing results (if applicable)
   - Verify prompt clarity and documentation quality
   - Indicate if new instructions affect existing workflows
   - Resolve all comments before the PR is merged
