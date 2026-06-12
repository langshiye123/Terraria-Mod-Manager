# 🎮 Terraria Mod Manager

An **in-game mod manager for tModLoader** — manage your mods, profiles, and backups without ever leaving Terraria.

> **Also available: [CLI Version](https://github.com/langshiye123/Terraria-Mod-Manager/tree/master)** — external command-line tool for file-based mod management.

## ✨ Features

- **📊 Mod Dashboard** — View all installed mods with name, version, author, and load status in a clean in-game panel
- **💾 Mod Profiles** — Save your current enabled mods as named profiles; switch between mod sets for different playthroughs (e.g., "Vanilla+", "Calamity", "Thorium")
- **🔄 Backup System** — One-click backup of your mod configuration (`enabled.json`), with world and player save tracking
- **🛡️ Safe Restore** — Restore from any backup; a safety backup is automatically created before any restore operation
- **⚙️ ModConfig** — Configure auto-backup, max backup count, and UI preferences via tModLoader's built-in settings
- **💬 Chat Commands** — Quick actions via `/modmgr` commands in chat
- **🌍 Localization Ready** — English localization included, expandable to other languages

## 📋 Requirements

- **tModLoader** (1.4.4+ recommended, .NET 8.0 runtime)
- **Terraria** (Steam version)
- Works on Windows, Linux, and macOS

## 📥 Installation

### Steam Workshop (Recommended)

1. Subscribe to **Terraria Mod Manager** on the Steam Workshop
2. Enable the mod in tModLoader's Mods menu
3. Done!

### Manual Installation

1. Download the latest `.tmod` file from [GitHub Releases](https://github.com/langshiye123/Terraria-Mod-Manager/releases)
2. Place it in your tModLoader Mods folder:
   - **Windows**: `%USERPROFILE%\Documents\My Games\Terraria\tModLoader\Mods`
   - **Linux**: `~/.local/share/Terraria/tModLoader/Mods`
   - **macOS**: `~/Library/Application Support/Terraria/tModLoader/Mods`
3. Enable the mod in tModLoader's Mods menu

## 🎮 Usage

### In-Game Dashboard

The Mod Manager dashboard is accessible in two ways:

1. **Chat Command**: Type `/modmgr` in chat to open the management panel
2. **Config Menu**: Access settings via tModLoader's Settings → Mod Configuration → Terraria Mod Manager

### Dashboard Tabs

| Tab | What You Can Do |
|-----|-----------------|
| **Mod Dashboard** | Browse all installed mods with version, author, and load status |
| **Profiles** | Save current mods as a named profile, load saved profiles, compare differences |
| **Backups** | Create backups, view backup history, restore from previous backup |

### Chat Commands

```
/mmodmgr                    # Open the management dashboard
/modmgr backup              # Create a quick backup
/modmgr list-backups        # List all backups
/modmgr save-profile <name> # Save current mods as a profile
/modmgr list-profiles       # List all saved profiles
/modmgr help                # Show available commands
```

### Mod Profiles Workflow

1. Set up your mods for a specific playthrough (e.g., enable Calamity + its dependencies)
2. Type `/modmgr save-profile CalamityPlaythrough`
3. Later, when you want to switch: load the profile, compare differences, adjust mods in the Mods menu, and reload

## 📁 File Structure

All mod data is stored in `ModLoader/Mods/TerrariaModManager/`:

```
TerrariaModManager/
├── profiles/                  # Mod profiles (JSON files)
│   ├── CalamityPlaythrough.json
│   └── VanillaPlus.json
└── backups/                   # Backup snapshots
    ├── 2026-06-12_14-30-00/
    │   ├── enabled_mods.json      # Enabled mods list
    │   ├── mod_config_enabled.json # tModLoader mod config
    │   └── save_info.json          # World/player file manifest
    └── pre_restore_2026-06-12_15-00-00/  # Safety backup
```

## 🛡️ Safety Guarantees

| Operation | What Happens |
|-----------|-------------|
| **Backup** | Copies mod config and save manifests — originals untouched |
| **Restore** | Creates a `pre_restore_*` safety backup first, then applies changes |
| **Profile Load** | Shows differences only — never auto-enables/disables mods without your review |
| **Profile Delete** | Only removes the JSON profile file — your mods are never affected |

**Terraria Mod Manager never deletes your mods, worlds, or players.**

## 🔧 Developer Guide

### Building from Source

```bash
git clone https://github.com/langshiye123/Terraria-Mod-Manager.git
cd Terraria-Mod-Manager
git checkout tmodloader-mod
```

#### Method 1: tModLoader Built-in (Recommended)
1. Launch tModLoader → Workshop → Develop Mods
2. Open the cloned folder as a mod source
3. Click "Build + Reload"

#### Method 2: Manual Build
1. Ensure you have .NET 8.0 SDK installed
2. Adjust the `TerrariaModManager.csproj` to reference your tModLoader installation
3. Run `dotnet build`
4. Use tModLoader's mod packaging tool to create the `.tmod`

### Project Structure

```
Terraria-Mod-Manager/              (tmodloader-mod branch)
├── build.txt                      # Mod metadata
├── description.txt                # Mod description
├── TerrariaModManager.csproj      # Project file
├── ModManagerMod.cs               # Main Mod class
├── Common/
│   ├── Configs/
│   │   └── ModManagerConfig.cs    # ModConfig settings
│   └── Systems/
│       ├── ModManagerSystem.cs    # UI lifecycle & rendering
│       └── ChatCommandSystem.cs   # /modmgr chat commands
├── Core/
│   ├── ModInfoHelper.cs           # Mod info retrieval via tML API
│   ├── ProfileManager.cs          # Mod profile CRUD
│   └── BackupManager.cs           # Backup/restore logic
├── UI/
│   ├── ModDashboardUI.cs          # Main UI state with tabs
│   ├── DashboardPanel.cs          # Mod list panel
│   ├── ProfilePanel.cs            # Profile management panel
│   └── BackupPanel.cs             # Backup management panel
└── Localization/
    └── en-US.hjson                # English localization
```

### Tech Stack
- **Language**: C# 12
- **Framework**: tModLoader (targets .NET 8.0)
- **UI**: tModLoader UI system (UIState, UIElement, UIPanel)
- **Data**: JSON for profiles and backups
- **Configuration**: tModLoader ModConfig

## 🗺️ Roadmap

- [ ] Full world/player save backup (currently manifest-only)
- [ ] Auto-backup on mod reload
- [ ] Mod dependency resolution in profiles
- [ ] Profile sharing via Steam Workshop
- [ ] Mod update notifications
- [ ] Additional language translations (zh-CN, ru, pt-BR)

## 🔗 Links

- **CLI Version** (standalone tool): [master branch](https://github.com/langshiye123/Terraria-Mod-Manager/tree/master)
- **Release v1.0.0** (CLI, Win64): [Download](https://github.com/langshiye123/Terraria-Mod-Manager/releases/tag/v1.0.0)
- **ExampleMod Reference**: [tModLoader/ExampleMod](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod)
- **tModLoader Modding Guide**: [Basic tModLoader Modding Guide](https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide)

## 📄 License

MIT License — see [LICENSE](LICENSE) file.

---

Built with ❤️ for the Terraria modding community. Contributions welcome!
