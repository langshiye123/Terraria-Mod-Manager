using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaModManager.Core;

namespace TerrariaModManager.UI;

/// <summary>
/// 备份管理面板 — 创建、查看和恢复备份。
/// </summary>
public sealed class BackupListPanel : UIElement
{
    private readonly float _width;
    private readonly float _height;
    private UIList _backupList = null!;
    private UIScrollbar _scrollbar = null!;
    private UIText _statusText = null!;

    public BackupListPanel(float width, float height)
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

        // 安全声明
        var safetyText = new UIText(
            "SAFE: Backups never delete your data. Restore creates a safety backup first.",
            0.75f)
        {
            Top = StyleDimension.FromPixels(4f),
            Left = StyleDimension.FromPixels(10f),
            TextColor = Color.LimeGreen
        };
        Append(safetyText);

        // 按钮栏
        var buttonBar = new UIElement
        {
            Top = StyleDimension.FromPixels(28f),
            Width = StyleDimension.FromPixels(_width - 20f),
            Height = StyleDimension.FromPixels(36f),
            HAlign = 0.5f
        };
        Append(buttonBar);

        var createButton = CreateButton("Create Backup", 0f, 150f,
            new Color(40, 120, 60));
        createButton.OnLeftClick += (_, _) => CreateBackup();
        buttonBar.Append(createButton);

        var refreshButton = CreateButton("Refresh", 160f, 100f,
            new Color(60, 60, 120));
        refreshButton.OnLeftClick += (_, _) => Refresh();
        buttonBar.Append(refreshButton);

        // 备份列表
        _backupList = new UIList
        {
            Top = StyleDimension.FromPixels(70f),
            Width = StyleDimension.FromPixels(_width - 25f),
            Height = StyleDimension.FromPixels(_height - 115f),
            HAlign = 0.5f
        };
        Append(_backupList);

        // 滚动条
        _scrollbar = new UIScrollbar
        {
            Top = StyleDimension.FromPixels(70f),
            Left = StyleDimension.FromPixels(_width - 18f),
            Height = StyleDimension.FromPixels(_height - 115f),
            HAlign = 1f
        };
        _backupList.SetScrollbar(_scrollbar);
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
    /// 刷新备份列表。
    /// </summary>
    public void Refresh()
    {
        _backupList.Clear();

        var backups = BackupManager.ListBackups();

        foreach (var backup in backups)
        {
            var row = new UIElement
            {
                Width = StyleDimension.FromPixels(_width - 60f),
                Height = StyleDimension.FromPixels(32f)
            };

            // 备份摘要
            var summary = new UIText(
                $"{backup.Name}  |  {backup.ModCount} mods, {backup.WorldCount} worlds, {backup.PlayerCount} players",
                0.8f)
            {
                Top = StyleDimension.FromPixels(6f),
                Left = StyleDimension.FromPixels(4f),
                TextColor = Color.White
            };
            row.Append(summary);

            // 恢复按钮
            var restoreBtn = CreateSmallButton("Restore", _width - 230f, 3f,
                new Color(140, 120, 40));
            var capturedName = backup.Name;
            restoreBtn.OnLeftClick += (_, _) => RestoreBackup(capturedName);
            row.Append(restoreBtn);

            _backupList.Add(row);
        }

        _statusText.SetText(
            backups.Count == 0
                ? "No backups yet. Create your first backup to protect your mod setup."
                : $"{backups.Count} backup(s) available. Restore creates a safety backup first.");
    }

    private void CreateBackup()
    {
        try
        {
            var backup = BackupManager.CreateBackup();
            _statusText.SetText(
                $"Backup '{backup.Name}' created! ({backup.ModCount} mods, {backup.WorldCount} worlds, {backup.PlayerCount} players)");
            Refresh();
        }
        catch (Exception ex)
        {
            _statusText.SetText($"Backup failed: {ex.Message}");
        }
    }

    private void RestoreBackup(string backupName)
    {
        try
        {
            BackupManager.RestoreBackup(backupName);
            _statusText.SetText(
                $"Restored from '{backupName}'. A safety backup was created. Restart tModLoader to apply changes.");
            Refresh();
        }
        catch (Exception ex)
        {
            _statusText.SetText($"Restore failed: {ex.Message}");
        }
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
            Width = StyleDimension.FromPixels(56f),
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
