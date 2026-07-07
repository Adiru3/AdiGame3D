# AdiGame3D — 3D Game Engine

A complete 3D game engine featuring a map editor, custom 3D model loading, texture mapping, physics, spatial 3D audio, and multiplayer capabilities.  
Built on **C# / .NET Framework 4.6.1** using modern **OpenGL 3.3 Core Profile** via OpenTK 3.x.

---

## Project Structure

```
Adigame3d/
├── Engine.Core/        ← Shared core library: entities, scene serialization, JSON parser, network packets, OBJ & WAV loaders
├── Engine.Editor/      ← Map Editor (WinForms + OpenGL GLControl viewport)
└── Engine.Runtime/     ← Game Client / Player (OpenGL GameWindow, AABB physics, OpenAL audio, client-server network replication)
```

## System Requirements

- Windows 10 / 11 (64-bit)
- .NET Framework 4.6.1+ (integrated into Windows 10/11 by default)
- Visual Studio 2019/2022 or `dotnet` CLI / SDK
- Graphics card supporting **OpenGL 3.3** or higher

## Build Instructions

### Visual Studio
Open `Adigame3d.sln` → Build → Build Solution (`Ctrl+Shift+B`)

### Command Line
```cmd
dotnet restore Adigame3d.sln
dotnet build Adigame3d.sln -c Release
```

---

## Map Editor

Launch compiled editor from:
```
Engine.Editor/bin/Release/net461/Engine.Editor.exe
```

### Editor Controls
| Action | Key / Mouse |
|---|---|
| Rotate Camera | Right Mouse Button (RMB) + Move Mouse |
| Camera Forward / Backward | W / S |
| Camera Left / Right | A / D |
| Camera Up / Down | E / Q |
| Speed boost | Shift |
| Place block / entity | Left Mouse Button (LMB) in Place mode |
| Select block / entity | Left Mouse Button (LMB) in Select mode |
| Delete block / entity | Left Mouse Button (LMB) in Delete mode, or `Delete` key |
| Place Mode | 1 |
| Select Mode | 2 |
| Delete Mode | 3 |
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Save | Ctrl+S |
| Launch game runtime | F5 |

---

## Game Client (Runtime)

You can launch the standalone player from the command line:
```cmd
Engine.Runtime.exe level.json
```

### In-Game Controls
| Action | Key |
|---|---|
| Move | W / A / S / D |
| Jump | Space |
| Sprint | Shift |
| Capture mouse cursor | Left Click |
| Open Settings / Pause Menu | Escape |

### Settings & Pause Menu
Pressing **Escape** inside the game pauses physics and audio, and displays a settings overlay:
- **Master Volume:** Calibrate OpenAL listener sound levels.
- **Mouse Sensitivity:** Adjust FPS look speed.
- **Screen Resolution:** Change window dimensions on the fly.

### Multiplayer Configuration
```cmd
# Start as Host (Server + Player Client)
Engine.Runtime.exe level.json host 7777

# Connect to Host as Client
Engine.Runtime.exe level.json join 192.168.1.100 7777
```

---

## Custom Assets & Entities

All custom assets should be placed in the `Assets/` directory at the solution root:
- `Assets/Models/` — Custom 3D meshes in `.obj` format.
- `Assets/Textures/` — Image textures in `.png` or `.jpg` formats.
- `Assets/Sounds/` — Audio files in `.wav` format.

When exporting the game (**File → Export Game...**), the editor automatically packages the entire `Assets/` folder alongside the player executable and dependencies, making your game fully portable.

### Entity Types
- **Block & Terrain Materials:** Block, Stone, Wood, Glass, Metal, Brick, Grass, Sand, Water, Lava, Ice, Dirt.
- **Model3D (ID 12):** Renders a custom 3D mesh. Set the `ModelPath` and `TexturePath` properties in the properties grid.
- **SoundPoint (ID 105):** Plays spatial 3D audio. Configurable properties:
  - `SoundPath` — Path to `.wav` file.
  - `SoundRadius` — Roll-off max distance.
  - `SoundVolume` — Sound level (0.0 to 1.0).
  - `SoundLooping` — Toggle loop play.
- **CameraWaypoint (ID 106):** Defines path waypoints for camera fly-through cutscenes. Configurable properties:
  - `WaypointSequence` — Playback order (0, 1, 2, ...).
  - `WaypointDuration` — Time to interpolate to the next waypoint.
  - `WaypointFov` — Camera field of view at this waypoint.

---

## File Format

Levels are serialized into JSON:
```json
{
  "name": "My Level",
  "author": "Player1",
  "entities": [
    {
      "id": "787c8052-a5e2-45e0-9bc8-84221192e226",
      "type": "Model3D",
      "position": { "x": 5.0, "y": 0.0, "z": -2.0 },
      "rotation": { "x": 0.0, "y": 90.0, "z": 0.0 },
      "scale": { "x": 1.0, "y": 1.0, "z": 1.0 },
      "color": { "r": 1.0, "g": 1.0, "b": 1.0 },
      "properties": {
        "model_path": "Assets/Models/chair.obj",
        "texture_path": "Assets/Textures/wood.png"
      }
    }
  ]
}
```

---

## Technical Stack & Dependencies

| Dependency | Version | Purpose |
|---|---|---|
| **OpenTK** | 3.3.3 | OpenGL bindings, Windowing system, Math types (Matrix4, Vector3), OpenAL Audio |
| **OpenTK.GLControl** | 3.3.3 | WinForms integration viewport |
| **Newtonsoft.Json** | 13.0.3 | Scene state serialization/deserialization |
| **LiteNetLib** | 0.9.5.2 | Authoritative UDP multiplayer networking |

---

## ❤️ Support & Donation

If you find this project useful and would like to support its development, you can donate here:

👉 **[Donate Link](https://adiru3.github.io/Donate/)**
