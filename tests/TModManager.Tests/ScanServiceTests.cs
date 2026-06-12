using TModManager.Services;

namespace TModManager.Tests;

/// <summary>
/// ScanService 单元测试 — 使用临时目录，不依赖真实 Terraria 安装。
/// </summary>
public class ScanServiceTests
{
    [Fact]
    public void ScanMods_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var result = ScanService.ScanMods(nonExistentPath);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ScanMods_ReturnsEmptyList_WhenDirectoryIsEmpty()
    {
        using var tempDir = new TempDirectory();

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Empty(result);
    }

    [Fact]
    public void ScanMods_ReturnsMods_WhenTmodFilesExist()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod1.tmod"), "fake");
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod2.tmod"), "fake");

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.True(m.Enabled));
        Assert.Contains(result, m => m.FileName == "Mod1.tmod");
        Assert.Contains(result, m => m.FileName == "Mod2.tmod");
    }

    [Fact]
    public void ScanMods_IgnoresNonTmodFiles()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Mod1.tmod"), "fake");
        File.WriteAllText(Path.Combine(tempDir.Path, "readme.txt"), "text");
        File.WriteAllText(Path.Combine(tempDir.Path, "config.json"), "{}");

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Single(result);
        Assert.Equal("Mod1.tmod", result[0].FileName);
    }

    [Fact]
    public void ScanMods_DetectsDisabledMods()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "Enabled.tmod"), "fake");

        var disabledDir = Path.Combine(tempDir.Path, "disabled_mods");
        Directory.CreateDirectory(disabledDir);
        File.WriteAllText(Path.Combine(disabledDir, "Disabled.tmod"), "fake");

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Equal(2, result.Count);
        Assert.True(result.First(m => m.FileName == "Enabled.tmod").Enabled);
        Assert.False(result.First(m => m.FileName == "Disabled.tmod").Enabled);
    }

    [Fact]
    public void ScanMods_ReturnsCorrectFileMetadata()
    {
        using var tempDir = new TempDirectory();
        var filePath = Path.Combine(tempDir.Path, "TestMod.tmod");
        var content = new string('x', 1024); // 1KB
        File.WriteAllText(filePath, content);
        var expectedSize = new FileInfo(filePath).Length;

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Single(result);
        var mod = result[0];
        Assert.Equal("TestMod.tmod", mod.FileName);
        Assert.Equal(expectedSize, mod.SizeBytes);
        Assert.Equal(Path.GetFullPath(filePath), mod.FullPath);
    }

    [Fact]
    public void FormatAsTable_ReturnsNoModsMessage_WhenListIsEmpty()
    {
        var result = ScanService.FormatAsTable([]);

        Assert.Contains("未找到模组", result);
    }

    [Fact]
    public void FormatAsTable_IncludesSummary_WhenModsExist()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "ModA.tmod"), "x");
        var mods = ScanService.ScanMods(tempDir.Path);

        var result = ScanService.FormatAsTable(mods);

        Assert.Contains("ModA.tmod", result);
        Assert.Contains("Total:", result);
        Assert.Contains("Enabled:", result);
    }

    [Fact]
    public void FormatAsJson_ReturnsValidJson()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "ModA.tmod"), "x");
        var mods = ScanService.ScanMods(tempDir.Path);

        var result = ScanService.FormatAsJson(mods);

        Assert.Contains("\"fileName\": \"ModA.tmod\"", result);
        Assert.Contains("\"enabled\": true", result);
        Assert.StartsWith("[", result.Trim());
        Assert.EndsWith("]", result.Trim());
    }

    [Fact]
    public void ScanMods_ResultsAreSortedByFileName()
    {
        using var tempDir = new TempDirectory();
        File.WriteAllText(Path.Combine(tempDir.Path, "ZMod.tmod"), "z");
        File.WriteAllText(Path.Combine(tempDir.Path, "AMod.tmod"), "a");
        File.WriteAllText(Path.Combine(tempDir.Path, "MMod.tmod"), "m");

        var result = ScanService.ScanMods(tempDir.Path);

        Assert.Equal(3, result.Count);
        Assert.Equal("AMod.tmod", result[0].FileName);
        Assert.Equal("MMod.tmod", result[1].FileName);
        Assert.Equal("ZMod.tmod", result[2].FileName);
    }
}
