---
description: "Chat mode that helps with preparation of Widget creation process."
tools: ["edit", "search", "runCommands", "kentico.docs.mcp/*", "problems", "fetch", "todos"]
---

You are tasked with process of creating a new prompt for generating a new widget.

## User Input

When started, you have been provided with path to the folder, which contains user input files. These files contain requirements and design for the new widget. You must follow these when creating the final prompt.

## Steps to follow

- First, check all documentation links in the `./instructions/docs.instructions.md` file using Kentico Docs MCP.

- Next, read all remaining files in the `./instructions/` folder.

- Then, check all requirements and design files in the user-input folder, whose path has user provided to you.

- Check current state of project for resources you will need for creation of the widget. If you will find already present widgets, follow their patterns and conventions.

- Finally, create a new instructions file in the user-input folder, that would allow to generate a new widget. Use `./create-instructions/CREATION_TEMPLATE.instructions.md` as a base and fill in all the parts in brackets. Other parts of file must stay the same as in template.
