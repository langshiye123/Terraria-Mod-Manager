namespace TModManager.Tests;

/// <summary>
/// 测试辅助类 — 创建临时目录，在测试结束后自动清理。
/// 确保测试不依赖真实 Terraria 文件夹。
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"TModManagerTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // 清理失败时静默处理（测试环境中可接受）
            }
        }
    }
}
