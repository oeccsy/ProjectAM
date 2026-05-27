# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 project (Unity 6000.3.7f1) — a simulation application with a title screen entry point.

## Architecture

### Scene Management

This project does **not** use Unity's built-in SceneManager. Instead, "scenes" are prefabs loaded from `Assets/Resources/Prefabs/` and instantiated at runtime:

- `GameManager` (bootstraps the app) → loads `TitleScene` prefab
- `TitleScene` → on Start button press, loads `SimulationScene` prefab and destroys itself

To add a new scene: create a MonoBehaviour class, a prefab under `Assets/Resources/Prefabs/`, and wire up instantiation/destruction manually.

### AutoBind System

UI component references are bound via reflection rather than drag-and-drop in the Inspector:

1. Decorate a field or property with `[Bind]` (optionally pass a name override: `[Bind("ButtonName")]`)
2. The field/property type must be a `Component` subtype
3. In the Unity Editor, select the UI MonoBehaviour and click **Auto Bind** (visible on any MonoBehaviour whose class name contains `"UI"`)
4. The editor tool (`AutoBindEditor`) searches all child transforms for a name match and assigns the component

Example (from `TitleSceneUI`):
```csharp
[field: SerializeField] [Bind("StartButton")]
public Button StartButton { get; set; }
```

### Folder Structure

```
Assets/Scripts/
  Core/           # GameManager (app entry point)
  TItleScene/     # TitleScene + TitleSceneUI (note: typo in folder name is intentional)
  SimulationScene/# SimulationScene
  Utils/Attributes/  # BindAttribute
  Editors/        # AutoBindEditor (UNITY_EDITOR only)
Assets/Resources/Prefabs/  # Runtime-loadable scene prefabs
```

## Code Conventions

See [DOCS/CodeConventions.md](DOCS/CodeConventions.md) for coding style rules.

## Development Notes

- Open the project in Unity 6000.3.7f1 — do not upgrade the editor version without consideration.
- The target frame rate is locked to 60 fps in `GameManager.Start()`.
- `AutoBindEditor.cs` is wrapped in `#if UNITY_EDITOR` — keep editor-only code in `Assets/Scripts/Editors/` with this guard.
- The folder `TItleScene` has a capital `I` (typo) — match this exact casing when adding files to that folder.
