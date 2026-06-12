using TModManager.Services;

namespace TModManager.Tests;

/// <summary>
/// ModEnabler 单元测试 — 使用临时目录，验证启用/禁用行为。
/// </summary>
public class ModEnablerTests
{
    [Fact]
    public void DisableMod_MovesFileToDisabledDirectory()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        File.WriteAllText(Path.Combine(tempDir.Path, modFile), "fake content");

        ModEnabler.DisableMod(tempDir.Path, modFile);

        // 文件应该已从 Mods 目录移走
        Assert.False(File.Exists(Path.Combine(tempDir.Path, modFile)));
        // 文件应该出现在 disabled_mods/ 中
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "disabled_mods", modFile)));
    }

    [Fact]
    public void DisableMod_Throws_WhenFileDoesNotExist()
    {
        using var tempDir = new TempDirectory();

        var ex = Assert.Throws<FileNotFoundException>(
            () => ModEnabler.DisableMod(tempDir.Path, "NonExistent.tmod"));

        Assert.Contains("NonExistent.tmod", ex.Message);
    }

    [Fact]
    public void DisableMod_Throws_WhenTargetAlreadyExists()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        File.WriteAllText(Path.Combine(tempDir.Path, modFile), "original");

        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, modFile), "already disabled");

        var ex = Assert.Throws<IOException>(
            () => ModEnabler.DisableMod(tempDir.Path, modFile));

        Assert.Contains("已存在同名文件", ex.Message);
    }

    [Fact]
    public void DisableMod_DoesNotDeleteFile()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        var originalContent = "important mod data";
        File.WriteAllText(Path.Combine(tempDir.Path, modFile), originalContent);

        ModEnabler.DisableMod(tempDir.Path, modFile);

        // 内容应保持完整
        var movedContent = File.ReadAllText(
            Path.Combine(tempDir.Path, "disabled_mods", modFile));
        Assert.Equal(originalContent, movedContent);
    }

    [Fact]
    public void EnableMod_MovesFileBackToModsDirectory()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, modFile), "restored content");

        ModEnabler.EnableMod(tempDir.Path, modFile);

        // 文件应该已从 disabled_mods/ 移回 Mods 目录
        Assert.True(File.Exists(Path.Combine(tempDir.Path, modFile)));
        Assert.False(File.Exists(Path.Combine(disabledDir, modFile)));
    }

    [Fact]
    public void EnableMod_Throws_WhenDisabledFileDoesNotExist()
    {
        using var tempDir = new TempDirectory();

        var ex = Assert.Throws<FileNotFoundException>(
            () => ModEnabler.EnableMod(tempDir.Path, "NonExistent.tmod"));

        Assert.Contains("NonExistent.tmod", ex.Message);
    }

    [Fact]
    public void EnableMod_Throws_WhenTargetAlreadyExists()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        File.WriteAllText(Path.Combine(tempDir.Path, modFile), "already enabled");

        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, modFile), "disabled version");

        var ex = Assert.Throws<IOException>(
            () => ModEnabler.EnableMod(tempDir.Path, modFile));

        Assert.Contains("已存在同名文件", ex.Message);
    }

    [Fact]
    public void EnableMod_PreservesFileContent()
    {
        using var tempDir = new TempDirectory();
        var modFile = "TestMod.tmod";
        var originalContent = "precious mod data";
        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, modFile), originalContent);

        ModEnabler.EnableMod(tempDir.Path, modFile);

        var restoredContent = File.ReadAllText(Path.Combine(tempDir.Path, modFile));
        Assert.Equal(originalContent, restoredContent);
    }
}
