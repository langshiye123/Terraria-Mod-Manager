namespace TModManager.Services;

/// <summary>
/// 模组启用/禁用服务 — 通过移动文件到 disabled_mods/ 子目录实现，
/// 绝不删除用户的模组文件。
/// </summary>
public static class ModEnabler
{
    /// <summary>
    /// 禁用模组：将 .tmod 文件从 Mods 目录移动到 disabled_mods/ 子目录。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <param name="modFileName">模组文件名 (例如 "ExampleMod.tmod")</param>
    /// <exception cref="FileNotFoundException">源文件不存在</exception>
    /// <exception cref="IOException">目标位置已有同名文件</exception>
    public static void DisableMod(string modsPath, string modFileName)
    {
        var sourceFile = Path.Combine(modsPath, modFileName);

        if (!File.Exists(sourceFile))
            throw new FileNotFoundException($"模组文件不存在: {modFileName}");

        var disabledDir = PathService.GetDisabledModsPath(modsPath);
        PathService.EnsureDirectoryExists(disabledDir);

        var targetFile = Path.Combine(disabledDir, modFileName);

        if (File.Exists(targetFile))
            throw new IOException(
                $"disabled_mods/ 目录已存在同名文件，为避免覆盖已取消操作: {modFileName}");

        File.Move(sourceFile, targetFile);
    }

    /// <summary>
    /// 启用模组：将 .tmod 文件从 disabled_mods/ 子目录移回 Mods 目录。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <param name="modFileName">模组文件名 (例如 "ExampleMod.tmod")</param>
    /// <exception cref="FileNotFoundException">禁用的模组文件不存在</exception>
    /// <exception cref="IOException">Mods 目录已有同名文件</exception>
    public static void EnableMod(string modsPath, string modFileName)
    {
        var disabledDir = PathService.GetDisabledModsPath(modsPath);
        var sourceFile = Path.Combine(disabledDir, modFileName);

        if (!File.Exists(sourceFile))
            throw new FileNotFoundException($"禁用的模组文件不存在: {modFileName}");

        var targetFile = Path.Combine(modsPath, modFileName);

        if (File.Exists(targetFile))
            throw new IOException(
                $"Mods 目录已存在同名文件，为避免覆盖已取消操作: {modFileName}");

        File.Move(sourceFile, targetFile);
    }
}
