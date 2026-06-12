using TModManager.Services;

namespace TModManager.Tests;

/// <summary>
/// BackupService 单元测试 — 使用临时目录，验证备份/恢复行为。
/// </summary>
public class BackupServiceTests
{
    [Fact]
    public void CreateBackup_CreatesBackupDirectoryWithFiles()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod1.tmod"), "mod1");
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod2.tmod"), "mod2");
        // 非 .tmod 文件不应被备份
        File.WriteAllText(Path.Combine(tempDir.Path, "notes.txt"), "notes");

        var backup = BackupService.CreateBackup(tempDir.Path);

        Assert.True(Directory.Exists(backup.Path));
        Assert.Equal(2, backup.ModCount);
        Assert.True(File.Exists(Path.Combine(backup.Path, "Mod1.tmod")));
        Assert.True(File.Exists(Path.Combine(backup.Path, "Mod2.tmod")));
        // 非 .tmod 文件不应被备份
        Assert.False(File.Exists(Path.Combine(backup.Path, "notes.txt")));
    }

    [Fact]
    public void CreateBackup_ReturnsEmptyBackup_WhenNoModsExist()
    {
        using var tempDir = new TempDirectory();

        var backup = BackupService.CreateBackup(tempDir.Path);

        Assert.Equal(0, backup.ModCount);
        Assert.True(Directory.Exists(backup.Path));
    }

    [Fact]
    public void CreateBackup_DoesNotIncludeDisabledMods()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Enabled.tmod"), "enabled");

        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, "Disabled.tmod"), "disabled");

        var backup = BackupService.CreateBackup(tempDir.Path);

        Assert.Equal(1, backup.ModCount);
        Assert.True(File.Exists(Path.Combine(backup.Path, "Enabled.tmod")));
        Assert.False(File.Exists(Path.Combine(backup.Path, "Disabled.tmod")));
    }

    [Fact]
    public void ListBackups_ReturnsEmpty_WhenNoBackupsExist()
    {
        using var tempDir = new TempDirectory();

        var result = BackupService.ListBackups(tempDir.Path);

        Assert.Empty(result);
    }

    [Fact]
    public void ListBackups_ReturnsSortedByDateDescending()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), "content");

        // 创建两个备份
        var backup1 = BackupService.CreateBackup(tempDir.Path);
        Thread.Sleep(1100); // 确保时间戳不同
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod2.tmod"), "content2");
        var backup2 = BackupService.CreateBackup(tempDir.Path);

        var backups = BackupService.ListBackups(tempDir.Path);

        Assert.True(backups.Count >= 2);
        // 最新的备份应该在最前面
        Assert.Equal(backup2.Name, backups[0].Name);
    }

    [Fact]
    public void RestoreBackup_RestoresFilesCorrectly()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), "original content");

        var backup = BackupService.CreateBackup(tempDir.Path);

        // 修改原始文件以模拟变更
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), "modified content");
        File.WriteAllText(Path.Combine(tempDir.Path, "NewMod.tmod"), "new mod");

        // 恢复
        BackupService.RestoreBackup(tempDir.Path, backup.Name);

        // 验证恢复后的状态
        var restoredContent = File.ReadAllText(Path.Combine(tempDir.Path, "Mod.tmod"));
        Assert.Equal("original content", restoredContent);
        // 新添加的模组应该被移除
        Assert.False(File.Exists(Path.Combine(tempDir.Path, "NewMod.tmod")));
    }

    [Fact]
    public void RestoreBackup_CreatesSafetyBackup()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), "original");

        var backup = BackupService.CreateBackup(tempDir.Path);

        // 修改文件
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), "modified");

        // 恢复，应创建 pre_restore 备份
        BackupService.RestoreBackup(tempDir.Path, backup.Name);

        var backups = BackupService.ListBackups(tempDir.Path);
        Assert.Contains(backups, b => b.Name.StartsWith("pre_restore_"));
    }

    [Fact]
    public void RestoreBackup_Throws_WhenBackupDoesNotExist()
    {
        using var tempDir = new TempDirectory();

        var ex = Assert.Throws<DirectoryNotFoundException>(
            () => BackupService.RestoreBackup(tempDir.Path, "nonexistent_backup"));

        Assert.Contains("nonexistent_backup", ex.Message);
    }

    [Fact]
    public void CreateBackup_PreservesFileContent()
    {
        using var tempDir = new TempDirectory();
        var content = "precise mod content for verification";
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod.tmod"), content);

        var backup = BackupService.CreateBackup(tempDir.Path);

        var backedUpContent = File.ReadAllText(Path.Combine(backup.Path, "Mod.tmod"));
        Assert.Equal(content, backedUpContent);
    }
}
