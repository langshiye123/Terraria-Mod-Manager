namespace TModManager.Models;

/// <summary>
/// 模组信息记录，包含文件元数据和启用状态。
/// </summary>
public record ModInfo(
    string FileName,
    string FullPath,
    long SizeBytes,
    DateTime LastModified,
    bool Enabled
);
