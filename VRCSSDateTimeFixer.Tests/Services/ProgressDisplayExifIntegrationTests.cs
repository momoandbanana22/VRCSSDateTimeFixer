using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services;

public class ProgressDisplayExifIntegrationTests
{
    private static string CreateTestImage(string directory, string fileNameWithExt)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileNameWithExt);
        using (var image = new Image<Rgba32>(16, 16))
        {
            image.Save(path);
        }
        return path;
    }

    [Fact]
    public async Task ProcessFileAsync_WhenExifUpdateFails_DisplaysSkip()
    {
        // Arrange: コンソール出力のキャプチャを先に設定（Program の静的 ProgressDisplay 初期化前）
        var sw = new StringWriter();
        Console.SetOut(sw);
        Console.SetError(sw);

        var dir = Path.Combine(Path.GetTempPath(), "vrcss_progress_tests");
        var path = CreateTestImage(dir, "VRChat_1920x1080_2022-08-31_21-54-39.227.png");

        await using var lockStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None // 排他ロックで Exif 更新を失敗させる
        );

        // Act: Exif 更新が false となり、表示は「撮影日時：スキップ」のはず
        await VRCSSDateTimeFixer.Program.ProcessFileAsync(path);

        // Assert
        var output = sw.ToString();
        Assert.Contains("撮影日時：スキップ", output);
    }
}
