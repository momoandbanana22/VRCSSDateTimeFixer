using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VRCSSDateTimeFixer;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services;

public class FileTimestampUpdaterExifTests
{
    private static string CreateTestImage(string directory, string fileNameWithExt)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileNameWithExt);
        using (var image = new Image<Rgba32>(32, 32))
        {
            image.Save(path); // 拡張子から自動判定
        }
        return path;
    }

    [Fact]
    public async Task UpdateExifDateAsync_LockedFile_ReturnsFalse()
    {
        // Arrange: テスト用JPEGを作成（VRChat 名称で日時抽出可能）
        var dir = Path.Combine(Path.GetTempPath(), "vrcss_exif_tests");
        var path = CreateTestImage(dir, "VRChat_1920x1080_2022-08-31_21-54-39.227.jpg");

        await using var lockStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None // 排他ロック
        );

        // Act
        var result = await FileTimestampUpdater.UpdateExifDateAsync(path);

        // Assert
        Assert.False(result);
    }
}
