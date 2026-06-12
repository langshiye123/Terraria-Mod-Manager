using TModManager.Services;

namespace TModManager;

/// <summary>
/// TModManager — tModLoader 模组管理器 CLI。
/// 支持扫描、启用/禁用、备份/恢复模组。
/// 绝不删除用户的模组文件。
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            var command = args[0].ToLowerInvariant();
            var options = ParseOptions(args[1..]);

            return command switch
            {
                "scan" => HandleScan(options),
                "enable" => HandleEnable(options),
                "disable" => HandleDisable(options),
                "backup" => HandleBackup(options),
                "restore" => HandleRestore(options),
                "list-backups" => HandleListBackups(options),
                "help" or "--help" or "-h" => PrintUsageAndReturn(),
                _ => HandleUnknown(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    // ─── Command Handlers ───────────────────────────────────────────

    private static int HandleScan(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);
        var format = options.TryGetValue("format", out var f) ? f.ToLowerInvariant() : "table";

        var mods = ScanService.ScanMods(modsPath);

        var output = format switch
        {
            "json" => ScanService.FormatAsJson(mods),
            _ => ScanService.FormatAsTable(mods)
        };

        Console.WriteLine(output);
        return 0;
    }

    private static int HandleEnable(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);

        if (!options.TryGetValue("_arg0", out var modName) || string.IsNullOrWhiteSpace(modName))
        {
            Console.Error.WriteLine("Error: Please specify a mod file name to enable.");
            Console.Error.WriteLine("Usage: TModManager enable <mod-name> [--path <mods-dir>]");
            return 1;
        }

        ModEnabler.EnableMod(modsPath, modName);
        Console.WriteLine($"Enabled: {modName}");
        return 0;
    }

    private static int HandleDisable(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);

        if (!options.TryGetValue("_arg0", out var modName) || string.IsNullOrWhiteSpace(modName))
        {
            Console.Error.WriteLine("Error: Please specify a mod file name to disable.");
            Console.Error.WriteLine("Usage: TModManager disable <mod-name> [--path <mods-dir>]");
            return 1;
        }

        ModEnabler.DisableMod(modsPath, modName);
        Console.WriteLine($"Disabled: {modName}");
        return 0;
    }

    private static int HandleBackup(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);
        var backup = BackupService.CreateBackup(modsPath);

        Console.WriteLine($"Backup created: {backup.Name}");
        Console.WriteLine($"  Path: {backup.Path}");
        Console.WriteLine($"  Mods backed up: {backup.ModCount}");

        return 0;
    }

    private static int HandleRestore(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);

        if (!options.TryGetValue("_arg0", out var backupName) || string.IsNullOrWhiteSpace(backupName))
        {
            Console.Error.WriteLine("Error: Please specify a backup name to restore.");
            Console.Error.WriteLine("Usage: TModManager restore <backup-name> [--path <mods-dir>]");
            return 1;
        }

        Console.WriteLine($"WARNING: This will replace current mods with backup '{backupName}'.");
        Console.WriteLine("A safety backup of current mods will be created before restore.");
        Console.Write("Continue? [y/N]: ");

        var response = Console.ReadLine();
        if (response?.Trim().ToLowerInvariant() != "y")
        {
            Console.WriteLine("Restore cancelled.");
            return 0;
        }

        BackupService.RestoreBackup(modsPath, backupName);
        Console.WriteLine($"Restored from backup: {backupName}");

        return 0;
    }

    private static int HandleListBackups(Dictionary<string, string> options)
    {
        var modsPath = GetModsPath(options);
        var backups = BackupService.ListBackups(modsPath);

        if (backups.Count == 0)
        {
            Console.WriteLine("No backups found.");
            return 0;
        }

        Console.WriteLine($"{"Backup Name",-25} {"Created",-22} {"Mods",-6}");
        Console.WriteLine(new string('-', 55));

        foreach (var b in backups)
        {
            Console.WriteLine($"{b.Name,-25} {b.CreatedAt:yyyy-MM-dd HH:mm:ss}   {b.ModCount,-6}");
        }

        Console.WriteLine(new string('-', 55));
        Console.WriteLine($"Total: {backups.Count} backup(s)");

        return 0;
    }

    private static int PrintUsageAndReturn()
    {
        PrintUsage();
        return 0;
    }

    private static int HandleUnknown(string command)
    {
        Console.Error.WriteLine($"Error: Unknown command '{command}'.");
        Console.Error.WriteLine("Run 'TModManager help' for usage information.");
        return 1;
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private static string GetModsPath(Dictionary<string, string> options)
    {
        options.TryGetValue("path", out var cliPath);
        return PathService.ResolveModsPath(cliPath);
    }

    /// <summary>
    /// 解析命令行参数。支持 --key value 和位置参数。
    /// 位置参数映射为 _arg0, _arg0 等。
    /// </summary>
    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>();
        var positionalIndex = 0;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    ? args[++i]
                    : "true";
                options[key] = value;
            }
            else if (args[i].StartsWith("-"))
            {
                var key = args[i][1..];
                var value = (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    ? args[++i]
                    : "true";
                options[key] = value;
            }
            else
            {
                options[$"_arg{positionalIndex++}"] = args[i];
            }
        }

        return options;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("TModManager — tModLoader Mod Manager");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  TModManager <command> [options]");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  scan          Scan and list all mods");
        Console.WriteLine("  enable        Enable a disabled mod");
        Console.WriteLine("  disable       Disable an enabled mod");
        Console.WriteLine("  backup        Create a backup of current mods");
        Console.WriteLine("  restore       Restore mods from a backup");
        Console.WriteLine("  list-backups  List all backups");
        Console.WriteLine("  help          Show this help message");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  --path <dir>  Specify tModLoader Mods directory");
        Console.WriteLine("  --format      Output format for scan: table (default) or json");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  TModManager scan");
        Console.WriteLine("  TModManager scan --format json");
        Console.WriteLine("  TModManager scan --path \"D:\\Terraria\\Mods\"");
        Console.WriteLine("  TModManager disable ExampleMod.tmod");
        Console.WriteLine("  TModManager enable ExampleMod.tmod");
        Console.WriteLine("  TModManager backup");
        Console.WriteLine("  TModManager restore 2026-06-12_14-30-00");
        Console.WriteLine("  TModManager list-backups");
        Console.WriteLine();
        Console.WriteLine("SAFETY: TModManager never deletes your mod files.");
        Console.WriteLine("Disabled mods are moved to disabled_mods/; restore creates safety backups.");
    }
}
