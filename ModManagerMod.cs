using Terraria.ModLoader;
using TerrariaModManager.Core;

namespace TerrariaModManager;

/// <summary>
/// Terraria Mod Manager — 游戏内模组管理仪表板。
/// 提供模组查看、配置方案、备份/恢复功能。
/// </summary>
public sealed class ModManagerMod : Mod
{
    /// <summary>模组数据存储根目录（<ModPath>/TerrariaModManager/）。</summary>
    internal static string DataPath => Path.Combine(ModLoader.ModPath, "TerrariaModManager");

    public override void Load()
    {
        // 初始化数据目录结构
        Directory.CreateDirectory(Path.Combine(DataPath, "profiles"));
        Directory.CreateDirectory(Path.Combine(DataPath, "backups"));
    }

    public override void Unload()
    {
        // 静态资源由各自的 Unload 方法清理
    }
}
