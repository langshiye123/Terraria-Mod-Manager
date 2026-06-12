# 🎮 Terraria Mod Manager

A command-line mod manager for Terraria tModLoader — safely scan, enable/disable, backup, and restore your mods without ever deleting them.

## ✨ Features

- **🔍 Scan Mods** — Scan `.tmod` files and display file name, size, modification time, and enabled/disabled status
- **📊 Dual Output** — Table format for readability or JSON for programmatic use
- **✅ Enable/Disable** — Disable mods by moving them to `disabled_mods/` (never deleted), enable them by moving back
- **💾 Backup** — Create timestamped backups of your current mod setup
- **🔄 Restore** — Restore from any previous backup, with automatic safety backup before restoration
- **📂 List Backups** — View all backups with creation time and mod count
- **🛡️ Safe by Design** — Never deletes your mod files; disabled mods are preserved, restores create safety backups
- **🌍 Cross-Platform** — Windows, Linux, and macOS support with automatic path detection

## 📋 System Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## 📦 Installation

### Build from Source

```bash
git clone https://github.com/langshiye123/Terraria-Mod-Manager.git
cd Terraria-Mod-Manager
dotnet build -c Release
```

The compiled binary will be at:
```
src/TModManager/bin/Release/net8.0/TModManager.exe   (Windows)
src/TModManager/bin/Release/net8.0/TModManager        (Linux/macOS)
```

### Publish as Single File (Optional)

```bash
dotnet publish src/TModManager -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## 📖 Usage

### Scan Mods

```bash
# Table format (default)
TModManager scan

# JSON format
TModManager scan --format json

# Specify custom Mods directory
TModManager scan --path "D:\Terraria\tModLoader\Mods"
```

**Example output (table):**
```
Status   File Name                                  Size         Last Modified
----------------------------------------------------------------------------------
Enabled  CalamityMod.tmod                           45.2 MB      2026-05-15 14:30:22
Enabled  ThoriumMod.tmod                            18.7 MB      2026-05-10 09:15:00
Disabled RecipeBrowser.tmod                         2.1 MB       2026-04-20 11:00:00
----------------------------------------------------------------------------------
Total: 3 mods (Enabled: 2, Disabled: 1)
```

### Enable / Disable Mods

```bash
# Disable a mod (moved to disabled_mods/)
TModManager disable ExampleMod.tmod

# Enable a mod (moved back to Mods/)
TModManager enable ExampleMod.tmod

# With custom path
TModManager disable ExampleMod.tmod --path "/home/user/tModLoader/Mods"
```

### Backup

```bash
# Create a backup of current mods
TModManager backup

# List all backups
TModManager list-backups
```

### Restore

```bash
# Restore from a backup (interactive confirmation required)
TModManager restore 2026-06-12_14-30-00
```

> ⚠️ **Restore will**:
> 1. Create a safety backup of your current mods (`pre_restore_*`)
> 2. Replace current mods with the backup contents
> 3. Ask for confirmation before proceeding

## 📁 tModLoader Mods Directory

The tool automatically detects your Mods folder. Detection order:

| Priority | Source | Description |
|----------|--------|-------------|
| 1 | `--path` argument | Explicitly specified path |
| 2 | `TMOD_MODS_PATH` env var | Environment variable |
| 3 | OS default path | Platform-specific default |

### Default Paths by OS

| OS | Default Mods Path |
|----|-------------------|
| **Windows** | `%USERPROFILE%\Documents\My Games\Terraria\tModLoader\Mods` |
| **Linux** | `~/.local/share/Terraria/tModLoader/Mods` |
| **macOS** | `~/Library/Application Support/Terraria/tModLoader/Mods` |

### Directory Structure After Using TModManager

```
Mods/
├── CalamityMod.tmod          # Enabled mods
├── ThoriumMod.tmod
├── disabled_mods/            # Disabled mods (preserved, not deleted)
│   └── OldMod.tmod
└── backups/                  # Backup snapshots
    ├── 2026-06-12_14-30-00/
    │   ├── CalamityMod.tmod
    │   └── ThoriumMod.tmod
    └── pre_restore_2026-06-12_15-00-00/
        └── CalamityMod.tmod
```

## 🛡️ Safety Guarantees

**TModManager never deletes your mod files.**

| Operation | What Actually Happens |
|-----------|----------------------|
| **Disable** | File is **moved** to `disabled_mods/` — not deleted |
| **Enable** | File is **moved** back to Mods directory |
| **Restore** | A **safety backup** is created before overwriting anything |
| **Backup** | Files are **copied** — originals untouched |

- No destructive operations exist in this tool
- Overwrite protection: operations abort if a file already exists at the target location
- Interactive confirmation required before restore

## 🔧 Developer Guide

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

Tests use temporary directories (`Path.GetTempPath()`) — **no real Terraria installation required**.

### Project Structure

```
Terraria-Mod-Manager/
├── TModManager.sln
├── src/
│   └── TModManager/
│       ├── Program.cs              # CLI entry point & argument parsing
│       ├── Models/
│       │   ├── ModInfo.cs          # Mod metadata record
│       │   └── BackupInfo.cs       # Backup metadata record
│       └── Services/
│           ├── PathService.cs      # Path detection & resolution
│           ├── ScanService.cs      # .tmod file scanning
│           ├── ModEnabler.cs       # Enable/disable operations
│           └── BackupService.cs    # Backup/restore operations
└── tests/
    └── TModManager.Tests/
        ├── TempDirectory.cs        # Test utility for temp directories
        ├── ScanServiceTests.cs     # Scan tests (10 tests)
        ├── ModEnablerTests.cs      # Enable/disable tests (8 tests)
        └── BackupServiceTests.cs   # Backup/restore tests (9 tests)
```

### Tech Stack

- **Language**: C# 12
- **Runtime**: .NET 8.0
- **Test Framework**: xUnit
- **Dependencies**: None (zero external packages beyond .NET BCL)

## 🗺️ Roadmap

- [ ] `install` command — download mods from Steam Workshop / tModLoader Mod Browser
- [ ] `update` command — check and update installed mods
- [ ] Configuration profiles — switch between mod sets for different playthroughs
- [ ] GUI frontend (WPF or Avalonia)
- [ ] Mod dependency resolution
- [ ] One-click Terraria launch with selected mods

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

Built with ❤️ for the Terraria modding community. Contributions welcome!
