# Widget creation

AI-assisted prompts for creating [Page Builder](https://docs.kentico.com/x/6QWiCQ) widgets in Xperience by Kentico.

## Workflow

These prompts provides two-stage AI assistance for building custom Page Builder widgets:

1. **Research stage** - Analyzes your requirements and design, validates them against Xperience documentation, and generates detailed implementation instructions
2. **Implementation stage** - Creates the widget code following the generated instructions and project conventions

## Prerequisites

- Xperience by Kentico project with Page Builder configured
- AI coding assistant installed (GitHub Copilot, Cursor, or Claude Desktop)
- Widget requirements file describing the main use cases and behavior
- Widget design file (optional, exported from Figma or similar)

## Usage

### 1. Copy the prompts to your project

Copy the appropriate files for your AI assistant:

**GitHub Copilot:**
```bash
cp -r src/widget-creation/gh-copilot/.github/* YOUR_PROJECT/.github/
```

**Cursor:**
```bash
cp -r src/widget-creation/cursor/.cursor/* YOUR_PROJECT/.cursor/
```

**Claude Desktop:**
```bash
cp -r src/widget-creation/claude-code/.claude/* YOUR_PROJECT/.claude/
```

This also adds the Xperience by Kentico [Documentation MCP server](https://docs.kentico.com/x/mcp_server_xp) to your workspace via the `.vscode/mcp.json` file.

### 2. Prepare context files

Create a folder with your widget requirements and design:

- **requirements.md** - Describes the widget functionality, presentation options, and technical requirements.
- **design.html** - Visual design and structure exported from Figma or other design tool (optional).

See the `examples/widget-creation/` directory for samples of these files.

### 3. Run the research stage

The AI analyzes your requirements, validates them against Xperience documentation, and creates a detailed instructions file in your input folder.

**VSCode Github Copilot example**

```
/widget-create-research

For the requirements described in: examples/widget-creation/requirements.md
```

### 4. Run the implementation stage

The AI creates the widget following the instructions, project conventions, and Xperience best practices.

Optional: The instructions file created in the research stage contains all the information required by the implementation stage. Depending on the scale of your project and the scope of the implementation, consider starting the implementation step from a new conversation to avoid possible LLM context degradation caused by excessive summarization.

**VSCode Github Copilot example**

```
/widget-create-implementation 

Follow instructions in: widget-creation/ARTICLE_SHOWCASE.instructions.md
```

## Prompt output

The implementation stage generates:

- Widget view component class
- Widget properties class
- Razor view file (`.cshtml`)
- View model class
- Localized resource strings (creates a .resx file and the corresponding registration class if none found in the workspace)

If your project already contains widgets, the prompt also mimics their implementation patterns and filesystem structure.

## Included files

### Instructions

These files provide context to the AI about Xperience by Kentico:

- `base-pagebuilder.instructions.md` - Core Page Builder concepts and APIs
- `docs.instructions.md` - Links to relevant Xperience documentation
- `example-widgets.instructions.md` - Examples of existing widget patterns

### Prompts/Commands

- **Research prompt** - Analyzes requirements and generates implementation instructions
- **Implementation prompt** - Creates the widget code based on instructions

### Template

- `CREATION_TEMPLATE.instructions.md` - Template for generating widget implementation instructions

## Best practices for usage

- Provide clear, specific requirements in your requirements file
- Include presentation options and error handling scenarios
- Review the generated instructions before running the implementation stage
- Thoroughly review and test the generated code

## Examples

See `examples/widget-creation/` for a complete example of context files for an article showcase widget, which includes:

- Structured requirements file
- Exported design HTML
