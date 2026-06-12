using System.Text.Json;

namespace TerrariaModManager.Core;

/// <summary>
/// 模组配置方案管理器 — 保存和加载启用模组列表方案。
/// 方案文件存储为 JSON 格式于 profiles/ 目录。
/// </summary>
public static class ProfileManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 获取所有配置方案列表。
    /// </summary>
    public static List<ProfileInfo> ListProfiles()
    {
        var profilesDir = Path.Combine(ModManagerMod.DataPath, "profiles");
        var profiles = new List<ProfileInfo>();

        if (!Directory.Exists(profilesDir))
            return profiles;

        foreach (var file in Directory.GetFiles(profilesDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<ProfileData>(json, JsonOptions);
                if (data != null)
                {
                    profiles.Add(new ProfileInfo(
                        Name: Path.GetFileNameWithoutExtension(file),
                        ModCount: data.Mods.Count,
                        CreatedAt: data.CreatedAt,
                        Description: data.Description ?? ""
                    ));
                }
            }
            catch
            {
                // 跳过损坏的方案文件
            }
        }

        return profiles.OrderByDescending(p => p.CreatedAt).ToList();
    }

    /// <summary>
    /// 保存当前启用的模组列表为配置方案。
    /// </summary>
    /// <param name="profileName">方案名称（不含扩展名）</param>
    /// <param name="description">方案描述</param>
    public static void SaveProfile(string profileName, string description = "")
    {
        var profilesDir = Path.Combine(ModManagerMod.DataPath, "profiles");
        Directory.CreateDirectory(profilesDir);

        var data = new ProfileData
        {
            Mods = ModInfoHelper.GetEnabledModNames(),
            CreatedAt = DateTime.Now,
            Description = description
        };

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var safeName = SanitizeFileName(profileName);
        File.WriteAllText(Path.Combine(profilesDir, $"{safeName}.json"), json);
    }

    /// <summary>
    /// 加载指定配置方案的模组列表。
    /// </summary>
    /// <returns>模组内部名称列表，如果方案不存在返回 null。</returns>
    public static List<string>? LoadProfile(string profileName)
    {
        var profilesDir = Path.Combine(ModManagerMod.DataPath, "profiles");
        var safeName = SanitizeFileName(profileName);
        var filePath = Path.Combine(profilesDir, $"{safeName}.json");

        if (!File.Exists(filePath))
            return null;

        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<ProfileData>(json, JsonOptions);
        return data?.Mods;
    }

    /// <summary>
    /// 删除指定配置方案。
    /// </summary>
    public static bool DeleteProfile(string profileName)
    {
        var profilesDir = Path.Combine(ModManagerMod.DataPath, "profiles");
        var safeName = SanitizeFileName(profileName);
        var filePath = Path.Combine(profilesDir, $"{safeName}.json");

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    /// <summary>
    /// 清除文件名中的不安全字符。
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}

/// <summary>
/// 配置方案持久化数据结构。
/// </summary>
internal class ProfileData
{
    public List<string> Mods { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// 配置方案摘要信息（用于列表展示）。
/// </summary>
public record ProfileInfo(
    string Name,
    int ModCount,
    DateTime CreatedAt,
    string Description
);
