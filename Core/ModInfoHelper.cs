using Terraria.ModLoader;

namespace TerrariaModManager.Core;

/// <summary>
/// 模组信息读取工具 — 通过 tModLoader API 获取已安装模组详情。
/// </summary>
public static class ModInfoHelper
{
    /// <summary>
    /// 获取当前加载的所有模组信息。
    /// </summary>
    public static List<ModDisplayInfo> GetAllMods()
    {
        var mods = new List<ModDisplayInfo>();

        foreach (var mod in ModLoader.Mods)
        {
            mods.Add(new ModDisplayInfo(
                InternalName: mod.Name,
                DisplayName: mod.DisplayName ?? mod.Name,
                Version: mod.Version?.ToString() ?? "?",
                Author: GetModAuthor(mod),
                IsLoaded: true
            ));
        }

        return mods.OrderBy(m => m.DisplayName).ToList();
    }

    /// <summary>
    /// 获取当前启用的模组内部名称列表（用于保存配置方案）。
    /// </summary>
    public static List<string> GetEnabledModNames()
    {
        return ModLoader.Mods.Select(m => m.Name).ToList();
    }

    /// <summary>
    /// 比较当前启用的模组和配置方案中的模组列表，返回差异。
    /// </summary>
    public static (List<string> ToEnable, List<string> ToDisable) CompareWithProfile(
        List<string> profileModNames)
    {
        var current = GetEnabledModNames().ToHashSet();
        var profile = profileModNames.ToHashSet();

        var toEnable = profile.Except(current).ToList();
        var toDisable = current.Except(profile).ToList();

        return (toEnable, toDisable);
    }

    private static string GetModAuthor(Mod mod)
    {
        // tModLoader 1.4+ 通过 Mod.DisplayName 或 build.txt 的 author 属性获取
        // 尝试通过反射获取作者信息（不同版本 API 略有差异）
        try
        {
            var buildProp = typeof(Mod)
                .GetProperty("Properties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(mod);
            if (buildProp != null)
            {
                var authorField = buildProp.GetType()
                    .GetProperty("author", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    ?.GetValue(buildProp);
                if (authorField != null)
                    return authorField.ToString() ?? "Unknown";
            }
        }
        catch
        {
            // 反射失败时返回默认值
        }
        return "Unknown";
    }
}

/// <summary>
/// 用于 UI 显示的模组信息。
/// </summary>
public record ModDisplayInfo(
    string InternalName,
    string DisplayName,
    string Version,
    string Author,
    bool IsLoaded
);
