using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaModManager.Core;

namespace TerrariaModManager.UI;

/// <summary>
/// 模组仪表板面板 — 显示所有已安装模组的列表和统计信息。
/// </summary>
public sealed class DashboardPanel : UIElement
{
    private readonly float _width;
    private readonly float _height;
    private UIList _modList = null!;
    private UIScrollbar _scrollbar = null!;
    private UIText _summaryText = null!;

    public DashboardPanel(float width, float height)
    {
        _width = width;
        _height = height;
        Width = StyleDimension.FromPixels(width);
        Height = StyleDimension.FromPixels(height);
        BuildUI();
    }

    private void BuildUI()
    {
        RemoveAllChildren();

        // 表头
        var headerY = 4f;
        Append(CreateLabel("Name", 10f, headerY, 250f, Color.Cyan));
        Append(CreateLabel("Version", 270f, headerY, 80f, Color.Cyan));
        Append(CreateLabel("Author", 360f, headerY, 200f, Color.Cyan));
        Append(CreateLabel("Status", 570f, headerY, 80f, Color.Cyan));

        // 分隔线
        var separator = new UIPanel
        {
            Top = StyleDimension.FromPixels(24f),
            Width = StyleDimension.FromPixels(_width - 30f),
            Height = StyleDimension.FromPixels(2f),
            HAlign = 0.5f,
            BackgroundColor = Color.Cyan * 0.5f,
            BorderColor = Color.Transparent
        };
        Append(separator);

        // 模组列表
        _modList = new UIList
        {
            Top = StyleDimension.FromPixels(28f),
            Width = StyleDimension.FromPixels(_width - 25f),
            Height = StyleDimension.FromPixels(_height - 70f),
            HAlign = 0.5f
        };
        Append(_modList);

        // 滚动条
        _scrollbar = new UIScrollbar
        {
            Top = StyleDimension.FromPixels(28f),
            Left = StyleDimension.FromPixels(_width - 18f),
            Height = StyleDimension.FromPixels(_height - 70f),
            HAlign = 1f
        };
        _modList.SetScrollbar(_scrollbar);
        Append(_scrollbar);

        // 摘要信息（底部）
        _summaryText = new UIText("Loading...", 0.85f)
        {
            Top = StyleDimension.FromPixels(_height - 28f),
            Left = StyleDimension.FromPixels(10f),
            TextColor = Color.LightGray
        };
        Append(_summaryText);
    }

    /// <summary>
    /// 刷新模组列表。
    /// </summary>
    public void Refresh()
    {
        _modList.Clear();

        var mods = ModInfoHelper.GetAllMods();

        foreach (var mod in mods)
        {
            var row = CreateModRow(mod);
            _modList.Add(row);
        }

        // 更新统计
        var enabled = mods.Count(m => m.IsLoaded);
        var total = mods.Count;
        _summaryText.SetText(
            $"Total: {total} mods  |  Loaded: {enabled}  |  " +
            $"Press /modmgr in chat or ESC to close");
    }

    private UIElement CreateModRow(ModDisplayInfo mod)
    {
        var row = new UIElement
        {
            Width = StyleDimension.FromPixels(_width - 30f),
            Height = StyleDimension.FromPixels(26f)
        };

        // 文件名（内部名称）
        row.Append(CreateLabel(mod.DisplayName, 0f, 4f, 250f, Color.White));
        // 版本
        row.Append(CreateLabel(mod.Version, 260f, 4f, 80f, Color.LightGray));
        // 作者
        row.Append(CreateLabel(mod.Author, 350f, 4f, 200f, Color.LightGray));
        // 状态
        var statusColor = mod.IsLoaded ? Color.LimeGreen : Color.OrangeRed;
        row.Append(CreateLabel(mod.IsLoaded ? "Loaded" : "Disabled", 560f, 4f, 80f, statusColor));

        return row;
    }

    private static UIText CreateLabel(string text, float left, float top, float width, Color color)
    {
        return new UIText(text, 0.8f)
        {
            Left = StyleDimension.FromPixels(left),
            Top = StyleDimension.FromPixels(top),
            Width = StyleDimension.FromPixels(width),
            TextColor = color
        };
    }
}
