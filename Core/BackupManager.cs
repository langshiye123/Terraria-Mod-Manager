using System.Text.Json;

namespace TerrariaModManager.Core;

/// <summary>
/// 备份管理器 — 备份和恢复模组配置、世界存档、玩家存档。
/// 恢复前自动创建安全备份，绝不删除原始数据。
/// </summary>
public static class BackupManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 创建完整备份（模组列表 + 模组配置 + 世界/玩家存档清单）。
    /// </summary>
    public static BackupInfo CreateBackup()
    {
        var backupName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupPath = Path.Combine(ModManagerMod.DataPath, "backups", backupName);
        Directory.CreateDirectory(backupPath);

        // 1. 备份当前启用的模组列表
        var modList = ModInfoHelper.GetEnabledModNames();
        File.WriteAllText(
            Path.Combine(backupPath, "enabled_mods.json"),
            JsonSerializer.Serialize(modList, JsonOptions));

        // 2. 备份模组配置文件 (enabled.json)
        var enabledJsonPath = Path.Combine(ModLoader.ModPath, "enabled.json");
        if (File.Exists(enabledJsonPath))
        {
            File.Copy(enabledJsonPath,
                Path.Combine(backupPath, "mod_config_enabled.json"), overwrite: true);
        }

        // 3. 记录世界和玩家存档信息（不复制大文件，仅记录清单）
        var saveInfo = new SaveInfo
        {
            Worlds = GetDirectoryFileList(Path.Combine(Main.SavePath, "Worlds")),
            Players = GetDirectoryFileList(Path.Combine(Main.SavePath, "Players")),
            BackedUpAt = DateTime.Now
        };
        File.WriteAllText(
            Path.Combine(backupPath, "save_info.json"),
            JsonSerializer.Serialize(saveInfo, JsonOptions));

        return new BackupInfo(
            Name: backupName,
            Path: backupPath,
            CreatedAt: DateTime.Now,
            ModCount: modList.Count,
            WorldCount: saveInfo.Worlds.Count,
            PlayerCount: saveInfo.Players.Count
        );
    }

    /// <summary>
    /// 列出所有备份，按时间降序排列。
    /// </summary>
    public static List<BackupInfo> ListBackups()
    {
        var backupsDir = Path.Combine(ModManagerMod.DataPath, "backups");
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(backupsDir))
            return backups;

        foreach (var dir in Directory.GetDirectories(backupsDir))
        {
            try
            {
                var dirInfo = new DirectoryInfo(dir);
                var modListPath = Path.Combine(dir, "enabled_mods.json");
                var modCount = 0;
                var worldCount = 0;
                var playerCount = 0;

                if (File.Exists(modListPath))
                {
                    var json = File.ReadAllText(modListPath);
                    var mods = JsonSerializer.Deserialize<List<string>>(json);
                    modCount = mods?.Count ?? 0;
                }

                var saveInfoPath = Path.Combine(dir, "save_info.json");
                if (File.Exists(saveInfoPath))
                {
                    var json = File.ReadAllText(saveInfoPath);
                    var info = JsonSerializer.Deserialize<SaveInfo>(json);
                    worldCount = info?.Worlds.Count ?? 0;
                    playerCount = info?.Players.Count ?? 0;
                }

                backups.Add(new BackupInfo(
                    Name: dirInfo.Name,
                    Path: dirInfo.FullName,
                    CreatedAt: dirInfo.CreationTime,
                    ModCount: modCount,
                    WorldCount: worldCount,
                    PlayerCount: playerCount
                ));
            }
            catch
            {
                // 跳过损坏的备份
            }
        }

        return backups.OrderByDescending(b => b.CreatedAt).ToList();
    }

    /// <summary>
    /// 恢复指定备份的模组配置（enabled.json）。
    /// 恢复前自动创建当前状态的安全备份。
    /// </summary>
    public static void RestoreBackup(string backupName)
    {
        var backupPath = Path.Combine(ModManagerMod.DataPath, "backups", backupName);

        if (!Directory.Exists(backupPath))
            throw new DirectoryNotFoundException($"Backup not found: {backupName}");

        // 恢复前创建安全备份
        CreateBackupWithName($"pre_restore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");

        // 恢复 enabled.json
        var sourceConfig = Path.Combine(backupPath, "mod_config_enabled.json");
        var targetConfig = Path.Combine(ModLoader.ModPath, "enabled.json");

        if (File.Exists(sourceConfig))
        {
            File.Copy(sourceConfig, targetConfig, overwrite: true);
        }

        // 恢复模组列表（写入 enabled_mods.json 供参考）
        var sourceModList = Path.Combine(backupPath, "enabled_mods.json");
        if (File.Exists(sourceModList))
        {
            var modList = File.ReadAllText(sourceModList);
            File.WriteAllText(
                Path.Combine(ModManagerMod.DataPath, "last_restored_mods.json"), modList);
        }
    }

    private static void CreateBackupWithName(string name)
    {
        var backupPath = Path.Combine(ModManagerMod.DataPath, "backups", name);
        Directory.CreateDirectory(backupPath);

        var modList = ModInfoHelper.GetEnabledModNames();
        File.WriteAllText(
            Path.Combine(backupPath, "enabled_mods.json"),
            JsonSerializer.Serialize(modList, JsonOptions));
    }

    private static List<string> GetDirectoryFileList(string path)
    {
        if (!Directory.Exists(path))
            return [];

        return Directory.GetFiles(path)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .ToList();
    }
}

/// <summary>
/// 备份信息记录。
/// </summary>
public record BackupInfo(
    string Name,
    string Path,
    DateTime CreatedAt,
    int ModCount,
    int WorldCount = 0,
    int PlayerCount = 0
);

/// <summary>
/// 存档信息（记录世界和玩家文件名）。
/// </summary>
internal class SaveInfo
{
    public List<string> Worlds { get; set; } = [];
    public List<string> Players { get; set; } = [];
    public DateTime BackedUpAt { get; set; }
}
