using Terraria.ModLoader;
using TerrariaModManager.Core;

namespace TerrariaModManager.Common.Systems;

/// <summary>
/// 聊天命令系统 — 注册 /modmgr 命令以快速执行模组管理操作。
/// </summary>
public sealed class ChatCommandSystem : ModSystem
{
    public override void Load()
    {
        // 聊天命令将在 PostSetupContent 后通过其他方式注册
    }

    /// <summary>
    /// 处理聊天命令。由外部调用或通过 tModLoader 的 CommandCaller 注册。
    /// 命令格式:
    ///   /modmgr                  - 打开管理面板
    ///   /modmgr dashboard        - 打开仪表板
    ///   /modmgr backup           - 创建备份
    ///   /modmgr list-backups     - 列出所有备份
    ///   /modmgr save-profile <name>  - 保存配置方案
    ///   /modmgr list-profiles    - 列出所有配置方案
    /// </summary>
    public static string HandleCommand(string[] args)
    {
        if (args.Length == 0 || args[0] == "dashboard")
        {
            ModManagerSystem.OpenUI();
            return "Mod Manager dashboard opened.";
        }

        return args[0].ToLowerInvariant() switch
        {
            "backup" => HandleBackup(),
            "list-backups" or "backups" => HandleListBackups(),
            "save-profile" => HandleSaveProfile(args),
            "list-profiles" or "profiles" => HandleListProfiles(),
            "help" => GetHelpText(),
            _ => $"Unknown subcommand: {args[0]}. Use /modmgr help for available commands."
        };
    }

    private static string HandleBackup()
    {
        try
        {
            var backup = BackupManager.CreateBackup();
            return $"Backup created: {backup.Name}\n" +
                   $"  Mods: {backup.ModCount}\n" +
                   $"  Worlds: {backup.WorldCount}\n" +
                   $"  Players: {backup.PlayerCount}";
        }
        catch (Exception ex)
        {
            return $"Backup failed: {ex.Message}";
        }
    }

    private static string HandleListBackups()
    {
        var backups = BackupManager.ListBackups();
        if (backups.Count == 0)
            return "No backups found.";

        var lines = new List<string> { $"Backups ({backups.Count}):" };
        foreach (var b in backups.Take(10))
        {
            lines.Add($"  {b.Name} — {b.ModCount} mods, {b.WorldCount} worlds, {b.PlayerCount} players");
        }
        return string.Join("\n", lines);
    }

    private static string HandleSaveProfile(string[] args)
    {
        if (args.Length < 2)
            return "Usage: /modmgr save-profile <name>";

        var name = args[1];
        var desc = args.Length > 2 ? string.Join(" ", args[2..]) : "";

        try
        {
            ProfileManager.SaveProfile(name, desc);
            var mods = ModInfoHelper.GetEnabledModNames();
            return $"Profile '{name}' saved with {mods.Count} mods.";
        }
        catch (Exception ex)
        {
            return $"Failed to save profile: {ex.Message}";
        }
    }

    private static string HandleListProfiles()
    {
        var profiles = ProfileManager.ListProfiles();
        if (profiles.Count == 0)
            return "No profiles found.";

        var lines = new List<string> { $"Profiles ({profiles.Count}):" };
        foreach (var p in profiles)
        {
            lines.Add($"  {p.Name} — {p.ModCount} mods ({p.CreatedAt:yyyy-MM-dd HH:mm})");
        }
        return string.Join("\n", lines);
    }

    private static string GetHelpText()
    {
        return "Terraria Mod Manager commands:\n" +
               "  /modmgr                  - Open management dashboard\n" +
               "  /modmgr backup           - Create a backup\n" +
               "  /modmgr list-backups      - List all backups\n" +
               "  /modmgr save-profile <n>  - Save current mods as a profile\n" +
               "  /modmgr list-profiles     - List all profiles";
    }
}
