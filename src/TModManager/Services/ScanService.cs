using System.Text;
using System.Text.Json;
using TModManager.Models;

namespace TModManager.Services;

/// <summary>
/// 扫描服务 — 扫描 Mods 目录下的 .tmod 文件，支持表格和 JSON 输出。
/// </summary>
public static class ScanService
{
    /// <summary>
    /// 扫描指定目录下的所有 .tmod 模组文件（包括启用和禁用的）。
    /// </summary>
    /// <param name="modsPath">Mods 目录路径</param>
    /// <returns>按文件名排序的模组信息列表</returns>
    public static List<ModInfo> ScanMods(string modsPath)
    {
        var mods = new List<ModInfo>();

        if (!Directory.Exists(modsPath))
            return mods;

        // 扫描启用的模组
        foreach (var file in Directory.GetFiles(modsPath, "*.tmod"))
        {
            mods.Add(CreateModInfo(file, enabled: true));
        }

        // 扫描禁用的模组
        var disabledPath = PathService.GetDisabledModsPath(modsPath);
        if (Directory.Exists(disabledPath))
        {
            foreach (var file in Directory.GetFiles(disabledPath, "*.tmod"))
            {
                mods.Add(CreateModInfo(file, enabled: false));
            }
        }

        return mods.OrderBy(m => m.FileName).ToList();
    }

    /// <summary>
    /// 以表格格式输出模组列表。
    /// </summary>
    public static string FormatAsTable(List<ModInfo> mods)
    {
        if (mods.Count == 0)
            return "未找到模组。";

        var sb = new StringBuilder();
        sb.AppendLine($"{"Status",-8} {"File Name",-42} {"Size",-12} {"Last Modified",-20}");
        sb.AppendLine(new string('-', 82));

        foreach (var mod in mods)
        {
            var status = mod.Enabled ? "Enabled" : "Disabled";
            var size = FormatSize(mod.SizeBytes);
            sb.AppendLine($"{status,-8} {mod.FileName,-42} {size,-12} {mod.LastModified:yyyy-MM-dd HH:mm:ss}");
        }

        sb.AppendLine(new string('-', 82));
        var enabled = mods.Count(m => m.Enabled);
        var disabled = mods.Count(m => !m.Enabled);
        sb.AppendLine($"Total: {mods.Count} mods (Enabled: {enabled}, Disabled: {disabled})");

        return sb.ToString();
    }

    /// <summary>
    /// 以 JSON 格式输出模组列表。
    /// </summary>
    public static string FormatAsJson(List<ModInfo> mods)
    {
        return JsonSerializer.Serialize(mods, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static ModInfo CreateModInfo(string filePath, bool enabled)
    {
        var fileInfo = new FileInfo(filePath);
        return new ModInfo(
            FileName: fileInfo.Name,
            FullPath: fileInfo.FullName,
            SizeBytes: fileInfo.Length,
            LastModified: fileInfo.LastWriteTime,
            Enabled: enabled
        );
    }

    private static string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        var order = 0;
        double size = bytes;
        while (size >= 1024 && order < units.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {units[order]}";
    }
}
