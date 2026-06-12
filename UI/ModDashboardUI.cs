using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaModManager.Core;

namespace TerrariaModManager.UI;

/// <summary>
/// 模组管理器主界面 — 包含仪表板、配置方案、备份三个标签页。
/// </summary>
public sealed class ModDashboardUI : UIState
{
    // 面板尺寸常量
    private const float PanelWidth = 700f;
    private const float PanelHeight = 520f;
    private const float TabHeight = 36f;

    private UIPanel _mainPanel = null!;
    private UIText _tabDashboard = null!;
    private UIText _tabProfiles = null!;
    private UIText _tabBackups = null!;
    private UIElement _contentArea = null!;

    private DashboardPanel _dashboardPanel = null!;
    private ProfileListPanel _profilePanel = null!;
    private BackupListPanel _backupPanel = null!;

    private string _activeTab = "dashboard";

    public void Initialize()
    {
        RemoveAllChildren();

        // 主面板
        _mainPanel = new UIPanel
        {
            Width = StyleDimension.FromPixels(PanelWidth),
            Height = StyleDimension.FromPixels(PanelHeight),
            HAlign = 0.5f,
            VAlign = 0.5f,
            BackgroundColor = new Color(33, 43, 79) * 0.9f,
            BorderColor = new Color(100, 140, 255) * 0.7f
        };
        Append(_mainPanel);

        // 标题
        var title = new UIText("Terraria Mod Manager", 1.3f)
        {
            HAlign = 0.5f,
            Top = StyleDimension.FromPixels(8f),
            TextColor = Color.Cyan
        };
        _mainPanel.Append(title);

        // 标签页栏
        CreateTabBar();

        // 内容区域
        _contentArea = new UIElement
        {
            Width = StyleDimension.FromPixels(PanelWidth - 20f),
            Height = StyleDimension.FromPixels(PanelHeight - TabHeight - 50f),
            HAlign = 0.5f,
            Top = StyleDimension.FromPixels(TabHeight + 10f)
        };
        _mainPanel.Append(_contentArea);

        // 初始化子面板
        _dashboardPanel = new DashboardPanel(PanelWidth - 20f, PanelHeight - TabHeight - 50f);
        _profilePanel = new ProfileListPanel(PanelWidth - 20f, PanelHeight - TabHeight - 50f);
        _backupPanel = new BackupListPanel(PanelWidth - 20f, PanelHeight - TabHeight - 50f);

        _contentArea.Append(_dashboardPanel);
        _contentArea.Append(_profilePanel);
        _contentArea.Append(_backupPanel);

        // 关闭按钮
        var closeButton = new UITextPanel<char>('X', 0.7f)
        {
            Width = StyleDimension.FromPixels(30f),
            Height = StyleDimension.FromPixels(30f),
            HAlign = 1f,
            Top = StyleDimension.FromPixels(4f),
            Left = StyleDimension.FromPixels(-36f),
            BackgroundColor = new Color(180, 60, 60) * 0.8f
        };
        closeButton.OnLeftClick += (_, _) => ModManagerSystem.CloseUI();
        _mainPanel.Append(closeButton);

        SwitchTab("dashboard");
    }

    /// <summary>
    /// 创建标签页导航栏。
    /// </summary>
    private void CreateTabBar()
    {
        var tabBar = new UIElement
        {
            Width = StyleDimension.FromPixels(PanelWidth - 16f),
            Height = StyleDimension.FromPixels(TabHeight),
            HAlign = 0.5f,
            Top = StyleDimension.FromPixels(32f)
        };
        _mainPanel.Append(tabBar);

        var tabWidth = (PanelWidth - 16f) / 3f;

        _tabDashboard = CreateTab("Mod Dashboard", 0f, tabWidth);
        _tabProfiles = CreateTab("Profiles", tabWidth, tabWidth);
        _tabBackups = CreateTab("Backups", tabWidth * 2f, tabWidth);

        _tabDashboard.OnLeftClick += (_, _) => SwitchTab("dashboard");
        _tabProfiles.OnLeftClick += (_, _) => SwitchTab("profiles");
        _tabBackups.OnLeftClick += (_, _) => SwitchTab("backups");

        tabBar.Append(_tabDashboard);
        tabBar.Append(_tabProfiles);
        tabBar.Append(_tabBackups);
    }

    private UIText CreateTab(string text, float left, float width)
    {
        return new UIText(text, 0.9f)
        {
            Left = StyleDimension.FromPixels(left),
            Width = StyleDimension.FromPixels(width),
            Height = StyleDimension.FromPixels(TabHeight),
            TextColor = Color.LightGray,
            HAlign = 0f
        };
    }

    /// <summary>
    /// 切换活动标签页。
    /// </summary>
    private void SwitchTab(string tab)
    {
        _activeTab = tab;

        // 更新标签样式
        _tabDashboard.TextColor = tab == "dashboard" ? Color.Cyan : Color.LightGray;
        _tabProfiles.TextColor = tab == "profiles" ? Color.Cyan : Color.LightGray;
        _tabBackups.TextColor = tab == "backups" ? Color.Cyan : Color.LightGray;

        // 切换面板可见性
        _dashboardPanel.SetVisible(tab == "dashboard");
        _profilePanel.SetVisible(tab == "profiles");
        _backupPanel.SetVisible(tab == "backups");

        // 刷新数据
        RefreshData();
    }

    /// <summary>
    /// 刷新所有面板数据。
    /// </summary>
    public void RefreshData()
    {
        if (_activeTab == "dashboard")
            _dashboardPanel.Refresh();
        else if (_activeTab == "profiles")
            _profilePanel.Refresh();
        else if (_activeTab == "backups")
            _backupPanel.Refresh();
    }
}

/// <summary>
/// UI 面板扩展方法。
/// </summary>
internal static class UIElementExtensions
{
    public static void SetVisible(this UIElement element, bool visible)
    {
        // 通过设置 IgnoresMouseInteraction 和透明度来控制可见性
        if (visible)
        {
            element.IgnoresMouseInteraction = false;
            element.Remove();
            if (element.Parent != null)
                element.Parent.Append(element);
        }
        else
        {
            element.IgnoresMouseInteraction = true;
            element.Remove();
        }
    }
}
