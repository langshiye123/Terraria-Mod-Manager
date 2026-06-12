using Terraria.ModLoader;
using Terraria.UI;
using TerrariaModManager.UI;

namespace TerrariaModManager.Common.Systems;

/// <summary>
/// 模组管理器主系统 — 管理 UI 生命周期、聊天命令和游戏钩子。
/// </summary>
public sealed class ModManagerSystem : ModSystem
{
    internal static ModDashboardUI DashboardUI = null!;
    internal static UserInterface DashboardInterface = null!;

    /// <summary>UI 是否可见。</summary>
    internal static bool IsUIVisible { get; private set; }

    public override void Load()
    {
        DashboardUI = new ModDashboardUI();
        DashboardUI.Initialize();

        DashboardInterface = new UserInterface();
        DashboardInterface.SetState(DashboardUI);
    }

    public override void Unload()
    {
        DashboardUI = null!;
        DashboardInterface = null!;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (IsUIVisible)
        {
            DashboardInterface?.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // 在资源栏层之后渲染我们的 UI
        var resourceBarIndex = layers.FindIndex(l => l.Name == "Vanilla: Resource Bars");
        if (resourceBarIndex == -1)
            resourceBarIndex = layers.Count - 1;

        layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
            "TerrariaModManager: Dashboard",
            DrawDashboardUI,
            InterfaceScaleType.UI));
    }

    private bool DrawDashboardUI()
    {
        if (IsUIVisible)
        {
            DashboardInterface.Draw(Main.spriteBatch, new GameTime());
        }
        return true;
    }

    /// <summary>
    /// 切换 UI 可见性。
    /// </summary>
    public static void ToggleUI()
    {
        IsUIVisible = !IsUIVisible;

        if (IsUIVisible)
        {
            DashboardUI.RefreshData();
            Main.playerInventory = false;
            Main.blockInput = true;
        }
        else
        {
            Main.blockInput = false;
        }
    }

    /// <summary>
    /// 打开 UI（如果已关闭）。
    /// </summary>
    public static void OpenUI()
    {
        if (!IsUIVisible)
            ToggleUI();
    }

    /// <summary>
    /// 关闭 UI（如果已打开）。
    /// </summary>
    public static void CloseUI()
    {
        if (IsUIVisible)
            ToggleUI();
    }
}
