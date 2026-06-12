namespace TModManager.Models;

/// <summary>
/// 备份信息记录，描述一次模组备份的元数据。
/// </summary>
public record BackupInfo(
    string Name,
    string Path,
    DateTime CreatedAt,
    int ModCount
);
