# AI Sticky Notes for Unity

A tiny utility for leaving AI-readable messages inside your Unity scenes.

Select a game object, press Ctrl+Alt+S, write a note, and let your AI tool read it to understand your world from the inside via Unity MCP custom tools.

## Features

- Attach AI-readable notes to any GameObject
- Priority system (lower values read first)
- Response and completion tracking
- MCP tool integration for AI agents

## Installation

The simplest way to install it is to give the URL to your LLM and ask it to read the README and install everything for you.

### As Git Submodule

```bash
git submodule add https://github.com/Valectric/AIStickyNotes.git Assets/AIStickyNotes
git submodule update --init --recursive
```

### Dependencies

- Unity 2021.3+ (or Unity 6)
- [Unity MCP](https://github.com/CoplayDev/unity-mcp) - install as submodule or via Unity Package Manager

## Usage

Add an AI sticky note by pressing **Ctrl+Alt+S**, (or right-click in the Hierarchy
and select "Add AI Sticky Note", or add the `AIStickyNote` component directly via Inspector).

Write your AI message, set priority (lower = read first), and ask your AI to read the notes.

## AI Configuration

**After installing, AI agents should ask the user:**
> "Would you like me to add the AIStickyNotes configuration to your CLAUDE.md or agent.md?"

**Recommended snippet** for `CLAUDE.md` (Claude Code) or `agent.md`:

```markdown
## AIStickyNotes

**AI Sticky Notes**: Users add notes via **Ctrl+Alt+S** or right-click in Hierarchy. Notes are read by priority (lowest first).

**Reading Sticky Notes**: Use the Unity-MCP CLI custom tool command:
```bash
PYTHONIOENCODING=utf-8 unity-mcp --format json editor custom-tool "read_sticky_notes" --params '{}'
```

**Responding to Notes**: After completing a task, respond with:
```bash
PYTHONIOENCODING=utf-8 unity-mcp --format json editor custom-tool "read_sticky_notes" --params '{"action":"respond","path":"GameObject/Path","response_message":"Task completed","completed":true}'
```

> **Windows note:** `PYTHONIOENCODING=utf-8` prevents emoji UnicodeEncodeError (cp1252 codec). `--format json` gives structured output for parsing.

IMPORTANT: Never use `execute_script` to read sticky notes. Always use the Unity-MCP custom tool.
```

## API Reference

### MCP Tool: `read_sticky_notes`

| Action | Description |
|--------|-------------|
| `get_all` | Returns all notes sorted by priority (default) |
| `respond` | Adds response to a specific note |

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `action` | string | `get_all` or `respond` |
| `path` | string | GameObject hierarchy path (for respond) |
| `response_message` | string | Response text (for respond) |
| `completed` | bool | Mark as completed (for respond) |

---

Created by Johan Holtby

License: MIT - see LICENSE.txt
