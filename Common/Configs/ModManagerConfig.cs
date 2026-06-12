using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaModManager.Common.Configs;

/// <summary>
/// 模组管理器客户端配置 — 控制 UI 行为和备份策略。
/// </summary>
public class ModManagerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("$Mods.TerrariaModManager.Config.Header.General")]

    [DefaultValue(true)]
    [Label("$Mods.TerrariaModManager.Config.AutoBackup.Label")]
    [Tooltip("$Mods.TerrariaModManager.Config.AutoBackup.Tooltip")]
    public bool AutoBackupOnWorldLoad;

    [DefaultValue(5)]
    [Range(1, 50)]
    [Label("$Mods.TerrariaModManager.Config.MaxBackups.Label")]
    [Tooltip("$Mods.TerrariaModManager.Config.MaxBackups.Tooltip")]
    public int MaxBackupCount;

    [Header("$Mods.TerrariaModManager.Config.Header.UI")]

    [DefaultValue(true)]
    [Label("$Mods.TerrariaModManager.Config.ShowModCount.Label")]
    [Tooltip("$Mods.TerrariaModManager.Config.ShowModCount.Tooltip")]
    public bool ShowModCountOnMenu;

    [DefaultValue(true)]
    [Label("$Mods.TerrariaModManager.Config.ConfirmRestore.Label")]
    [Tooltip("$Mods.TerrariaModManager.Config.ConfirmRestore.Tooltip")]
    public bool ConfirmBeforeRestore;
}
