using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaModManager.Core;

namespace TerrariaModManager.UI;

/// <summary>
/// 配置方案列表面板 — 管理模组配置方案的保存、加载和删除。
/// </summary>
public sealed class ProfileListPanel : UIElement
{
    private readonly float _width;
    private readonly float _height;
    private UIList _profileList = null!;
    private UIScrollbar _scrollbar = null!;
    private UIText _statusText = null!;

    public ProfileListPanel(float width, float height)
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

        // 说明文字
        var helpText = new UIText(
            "Profiles save your current enabled mods list. Switch between mod sets for different playthroughs.",
            0.8f)
        {
            Top = StyleDimension.FromPixels(4f),
            Left = StyleDimension.FromPixels(10f),
            TextColor = Color.LightGray
        };
        Append(helpText);

        // 按钮栏
        var buttonBar = new UIElement
        {
            Top = StyleDimension.FromPixels(30f),
            Width = StyleDimension.FromPixels(_width - 20f),
            Height = StyleDimension.FromPixels(36f),
            HAlign = 0.5f
        };
        Append(buttonBar);

        var saveButton = CreateButton("Save Current as Profile", 0f, 200f,
            new Color(40, 120, 60));
        saveButton.OnLeftClick += (_, _) => ShowSaveDialog();
        buttonBar.Append(saveButton);

        var refreshButton = CreateButton("Refresh", 210f, 100f,
            new Color(60, 60, 120));
        refreshButton.OnLeftClick += (_, _) => Refresh();
        buttonBar.Append(refreshButton);

        // 配置方案列表
        _profileList = new UIList
        {
            Top = StyleDimension.FromPixels(72f),
            Width = StyleDimension.FromPixels(_width - 25f),
            Height = StyleDimension.FromPixels(_height - 115f),
            HAlign = 0.5f
        };
        Append(_profileList);

        // 滚动条
        _scrollbar = new UIScrollbar
        {
            Top = StyleDimension.FromPixels(72f),
            Left = StyleDimension.FromPixels(_width - 18f),
            Height = StyleDimension.FromPixels(_height - 115f),
            HAlign = 1f
        };
        _profileList.SetScrollbar(_scrollbar);
        Append(_scrollbar);

        // 状态文字
        _statusText = new UIText("", 0.8f)
        {
            Top = StyleDimension.FromPixels(_height - 30f),
            Left = StyleDimension.FromPixels(10f),
            TextColor = Color.Yellow
        };
        Append(_statusText);
    }

    /// <summary>
    /// 刷新配置方案列表。
    /// </summary>
    public void Refresh()
    {
        _profileList.Clear();

        var profiles = ProfileManager.ListProfiles();
        var currentMods = ModInfoHelper.GetEnabledModNames();

        foreach (var profile in profiles)
        {
            var row = new UIElement
            {
                Width = StyleDimension.FromPixels(_width - 60f),
                Height = StyleDimension.FromPixels(36f)
            };

            // 方案信息
            var info = new UIText(
                $"{profile.Name}  |  {profile.ModCount} mods  |  {profile.CreatedAt:yyyy-MM-dd HH:mm}",
                0.8f)
            {
                Top = StyleDimension.FromPixels(2f),
                Left = StyleDimension.FromPixels(4f),
                TextColor = Color.White
            };
            row.Append(info);
            if (!string.IsNullOrEmpty(profile.Description))
            {
                var desc = new UIText(profile.Description, 0.7f)
                {
                    Top = StyleDimension.FromPixels(18f),
                    Left = StyleDimension.FromPixels(8f),
                    TextColor = Color.Gray
                };
                row.Append(desc);
            }

            // 加载按钮
            var loadBtn = CreateSmallButton("Load", _width - 230f, 4f,
                new Color(40, 100, 40));
            var capturedName = profile.Name;
            loadBtn.OnLeftClick += (_, _) => LoadProfile(capturedName);
            row.Append(loadBtn);

            // 删除按钮
            var deleteBtn = CreateSmallButton("Delete", _width - 170f, 4f,
                new Color(140, 50, 50));
            deleteBtn.OnLeftClick += (_, _) => DeleteProfile(capturedName);
            row.Append(deleteBtn);

            _profileList.Add(row);
        }

        var currentCount = currentMods.Count;
        _statusText.SetText(
            profiles.Count == 0
                ? $"No profiles saved. Currently {currentCount} mods enabled."
                : $"{profiles.Count} profile(s)  |  Currently {currentCount} mods enabled  |  Click 'Save' to create a new profile");
    }

    private void ShowSaveDialog()
    {
        // 使用默认名称保存当前模组配置
        var defaultName = $"Profile_{DateTime.Now:yyyyMMdd_HHmmss}";
        ProfileManager.SaveProfile(defaultName,
            $"Auto-saved: {ModInfoHelper.GetEnabledModNames().Count} mods enabled");
        Refresh();
        _statusText.SetText($"Profile '{defaultName}' saved!");
    }

    private void LoadProfile(string profileName)
    {
        var profileMods = ProfileManager.LoadProfile(profileName);
        if (profileMods == null)
        {
            _statusText.SetText($"Error: Profile '{profileName}' not found.");
            return;
        }

        var currentMods = ModInfoHelper.GetEnabledModNames();
        var (toEnable, toDisable) = ModInfoHelper.CompareWithProfile(profileMods);

        if (toEnable.Count == 0 && toDisable.Count == 0)
        {
            _statusText.SetText($"Profile '{profileName}' matches current mod setup. No changes needed.");
            return;
        }

        // 显示差异信息，提示用户手动调整后重载
        var msg = $"Profile '{profileName}' loaded.\n";
        if (toEnable.Count > 0)
            msg += $"Mods to enable ({toEnable.Count}): {string.Join(", ", toEnable.Take(5))}...\n";
        if (toDisable.Count > 0)
            msg += $"Mods to disable ({toDisable.Count}): {string.Join(", ", toDisable.Take(5))}...\n";
        msg += "Use the tModLoader Mods menu to adjust and reload.";

        _statusText.SetText(msg);
    }

    private void DeleteProfile(string profileName)
    {
        if (ProfileManager.DeleteProfile(profileName))
        {
            _statusText.SetText($"Profile '{profileName}' deleted.");
        }
        Refresh();
    }

    private static UIPanel CreateButton(string text, float left, float width, Color bgColor)
    {
        var btn = new UIPanel
        {
            Left = StyleDimension.FromPixels(left),
            Width = StyleDimension.FromPixels(width),
            Height = StyleDimension.FromPixels(32f),
            BackgroundColor = bgColor * 0.8f,
            BorderColor = Color.White * 0.2f
        };
        btn.Append(new UIText(text, 0.8f)
        {
            HAlign = 0.5f,
            VAlign = 0.5f,
            TextColor = Color.White
        });
        return btn;
    }

    private static UIPanel CreateSmallButton(string text, float left, float top, Color bgColor)
    {
        var btn = new UIPanel
        {
            Left = StyleDimension.FromPixels(left),
            Top = StyleDimension.FromPixels(top),
            Width = StyleDimension.FromPixels(54f),
            Height = StyleDimension.FromPixels(26f),
            BackgroundColor = bgColor * 0.8f,
            BorderColor = Color.White * 0.2f
        };
        btn.Append(new UIText(text, 0.7f)
        {
            HAlign = 0.5f,
            VAlign = 0.5f,
            TextColor = Color.White
        });
        return btn;
    }
}
