namespace TModManager.Services;

/// <summary>
/// 路径管理服务 — 解析和验证 tModLoader Mods 目录路径。
/// 优先级: CLI 参数 > 环境变量 > OS 默认路径
/// </summary>
public static class PathService
{
    /// <summary>
    /// 解析 Mods 目录路径。按优先级尝试 CLI 参数、环境变量、OS 默认路径。
    /// </summary>
    /// <param name="cliPath">命令行指定的路径 (可为 null)</param>
    /// <returns>解析后的绝对路径</returns>
    /// <exception cref="DirectoryNotFoundException">路径不存在时抛出</exception>
    public static string ResolveModsPath(string? cliPath)
    {
        // 1. CLI 参数优先
        if (!string.IsNullOrEmpty(cliPath))
        {
            var fullPath = Path.GetFullPath(cliPath);
            if (Directory.Exists(fullPath))
                return fullPath;

            throw new DirectoryNotFoundException(
                $"指定的 Mods 目录不存在: {fullPath}");
        }

        // 2. 环境变量
        var envPath = Environment.GetEnvironmentVariable("TMOD_MODS_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
            return Path.GetFullPath(envPath);

        // 3. OS 默认路径
        var defaultPath = GetDefaultModsPath();
        if (Directory.Exists(defaultPath))
            return defaultPath;

        throw new DirectoryNotFoundException(
            $"未找到 tModLoader Mods 目录。\n" +
            $"默认搜索路径: {defaultPath}\n" +
            $"请使用 --path 参数指定路径，或设置环境变量 TMOD_MODS_PATH。");
    }

    /// <summary>
    /// 获取当前操作系统的默认 tModLoader Mods 目录路径。
    /// </summary>
    public static string GetDefaultModsPath()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", "Terraria", "tModLoader", "Mods");

        if (OperatingSystem.IsLinux())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local", "share", "Terraria", "tModLoader", "Mods");

        if (OperatingSystem.IsMacOS())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "Terraria", "tModLoader", "Mods");

        throw new PlatformNotSupportedException("不支持当前操作系统。");
    }

    /// <summary>获取禁用模组存放目录路径。</summary>
    public static string GetDisabledModsPath(string modsPath) =>
        Path.Combine(modsPath, "disabled_mods");

    /// <summary>获取备份存放目录路径。</summary>
    public static string GetBackupsPath(string modsPath) =>
        Path.Combine(modsPath, "backups");

    /// <summary>确保目录存在，不存在则创建。</summary>
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
