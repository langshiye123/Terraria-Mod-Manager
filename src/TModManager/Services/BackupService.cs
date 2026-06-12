using TModManager.Models;

namespace TModManager.Services;

/// <summary>
/// 备份/恢复服务 — 创建、列出和恢复 Mods 目录备份。
/// 恢复前自动创建二次备份，确保数据安全。
/// </summary>
public static class BackupService
{
    /// <summary>
    /// 创建当前 Mods 目录的备份（仅备份启用的 .tmod 文件）。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <returns>创建的备份信息</returns>
    public static BackupInfo CreateBackup(string modsPath)
    {
        var backupName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupsDir = PathService.GetBackupsPath(modsPath);
        PathService.EnsureDirectoryExists(backupsDir);

        var backupPath = Path.Combine(backupsDir, backupName);
        Directory.CreateDirectory(backupPath);

        var modCount = 0;

        // 仅备份启用的 .tmod 文件（不备份 disabled_mods/ 和 backups/ 本身）
        if (Directory.Exists(modsPath))
        {
            foreach (var file in Directory.GetFiles(modsPath, "*.tmod"))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(backupPath, fileName));
                modCount++;
            }
        }

        return new BackupInfo(
            Name: backupName,
            Path: backupPath,
            CreatedAt: DateTime.Now,
            ModCount: modCount
        );
    }

    /// <summary>
    /// 列出所有备份，按创建时间降序排列。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <returns>备份信息列表</returns>
    public static List<BackupInfo> ListBackups(string modsPath)
    {
        var backupsDir = PathService.GetBackupsPath(modsPath);
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(backupsDir))
            return backups;

        foreach (var dir in Directory.GetDirectories(backupsDir))
        {
            var dirInfo = new DirectoryInfo(dir);
            var modCount = Directory.GetFiles(dir, "*.tmod").Length;

            backups.Add(new BackupInfo(
                Name: dirInfo.Name,
                Path: dirInfo.FullName,
                CreatedAt: dirInfo.CreationTime,
                ModCount: modCount
            ));
        }

        return backups.OrderByDescending(b => b.CreatedAt).ToList();
    }

    /// <summary>
    /// 从指定备份恢复模组。恢复前自动创建当前状态的二次备份。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <param name="backupName">备份名称 (目录名)</param>
    /// <exception cref="DirectoryNotFoundException">备份不存在</exception>
    public static void RestoreBackup(string modsPath, string backupName)
    {
        var backupsDir = PathService.GetBackupsPath(modsPath);
        var backupPath = Path.Combine(backupsDir, backupName);

        if (!Directory.Exists(backupPath))
            throw new DirectoryNotFoundException($"备份不存在: {backupName}");

        // 恢复前创建当前状态的二次备份（安全措施）
        CreateBackupWithName(modsPath,
            $"pre_restore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");

        // 删除当前启用的 .tmod 文件
        foreach (var file in Directory.GetFiles(modsPath, "*.tmod"))
        {
            File.Delete(file);
        }

        // 从备份复制文件到 Mods 目录
        foreach (var file in Directory.GetFiles(backupPath, "*.tmod"))
        {
            var fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(modsPath, fileName));
        }
    }

    private static void CreateBackupWithName(string modsPath, string backupName)
    {
        var backupsDir = PathService.GetBackupsPath(modsPath);
        PathService.EnsureDirectoryExists(backupsDir);

        var backupPath = Path.Combine(backupsDir, backupName);
        Directory.CreateDirectory(backupPath);

        if (Directory.Exists(modsPath))
        {
            foreach (var file in Directory.GetFiles(modsPath, "*.tmod"))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(backupPath, fileName));
            }
        }
    }
}
