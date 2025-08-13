using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    public class FileTimestampUpdaterTests
    {
        [Fact]
        public void 有効なファイル名から日時を抽出してタイムスタンプを更新できること()
        {
            // Given: テスト用の一時ファイルを作成
            string tempFile = Path.GetTempFileName();
            try
            {
                // テスト用のファイルを作成（既存のタイムスタンプを上書きするため）
                File.WriteAllText(tempFile, "test");

                // テスト対象の日時
                var expectedDate = new DateTime(2022, 8, 31, 21, 54, 39, 227);
                string tempDir = Path.GetDirectoryName(tempFile) ?? string.Empty;
                string testFileName = Path.Combine(tempDir, "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                File.Move(tempFile, testFileName);

                // When: タイムスタンプを更新
                bool result = FileTimestampUpdater.UpdateFileTimestamp(testFileName);

                // Then: 更新が成功し、タイムスタンプが正しく設定されていること
                Assert.True(result);
                var fileInfo = new FileInfo(testFileName);
                Assert.Equal(expectedDate, fileInfo.CreationTime);
                Assert.Equal(expectedDate, fileInfo.LastWriteTime);
            }
            finally
            {
                // テスト用ファイルの後片付け
                if (File.Exists(tempFile)) File.Delete(tempFile);
                string testFile = Path.Combine(Path.GetTempPath(), "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }


        [Fact]
        public void 有効なファイル名から撮影日時を更新できること()
        {
            // Given: テスト用の一時ファイルを作成
            string tempFile = Path.GetTempFileName();
            try
            {
                // テスト用のPNGファイルを作成
                using (var image = new Image<Rgba32>(100, 100))
                {
                    image.SaveAsPng(tempFile);
                }

                string testDir = Path.GetDirectoryName(tempFile) ?? string.Empty;
                string testFileName = Path.Combine(
                    testDir,
                    "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                File.Move(tempFile, testFileName);

                // 期待する日時
                var expectedDate = new DateTime(2022, 8, 31, 21, 54, 39, 227);

                // When: 撮影日時を更新
                bool result = FileTimestampUpdater.UpdateExifDate(testFileName);

                // Then: 更新が成功すること
                Assert.True(result);

                // Exifの撮影日時を検証
                using (var image = Image.Load(testFileName))
                {
                    var exifProfile = image.Metadata.ExifProfile;
                    Assert.NotNull(exifProfile);

                    // Exifタグの値を取得
                    if (!exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeTag))
                    {
                        Assert.True(false, "DateTimeOriginal tag not found in EXIF data");
                        return;
                    }

                    // タグの値を安全に取得
                    var dateTimeValue = dateTimeTag.GetValue();
                    if (dateTimeValue == null)
                    {
                        Assert.True(false, "DateTimeOriginal tag value is null");
                        return;
                    }

                    // 文字列に変換して日時にパース
                    var dateTimeStr = dateTimeValue.ToString();
                    if (string.IsNullOrEmpty(dateTimeStr))
                    {
                        Assert.True(false, "DateTimeOriginal string value is null or empty");
                        return;
                    }

                    var actualDate = DateTime.ParseExact(
                        dateTimeStr,
                        "yyyy:MM:dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);

                    Assert.Equal(expectedDate.Year, actualDate.Year);
                    Assert.Equal(expectedDate.Month, actualDate.Month);
                    Assert.Equal(expectedDate.Day, actualDate.Day);
                    Assert.Equal(expectedDate.Hour, actualDate.Hour);
                    Assert.Equal(expectedDate.Minute, actualDate.Minute);
                    Assert.Equal(expectedDate.Second, actualDate.Second);
                }
            }
            finally
            {
                // テスト用ファイルの後片付け
                if (File.Exists(tempFile)) File.Delete(tempFile);
                string testFile = Path.Combine(
                    Path.GetTempPath(),
                    "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        [Fact]
        public void 有効なPNGファイルのExif撮影日時を更新できること()
        {
            // Given: テスト用の一時PNGファイルを作成
            string tempFile = Path.GetTempFileName() + ".png";
            try
            {
                // テスト用の画像を作成
                using (var image = new Image<Rgba32>(100, 100))
                {
                    image.SaveAsPng(tempFile);
                }

                // テスト対象のファイル名（正しい形式）
                string testDir = Path.GetDirectoryName(tempFile) ?? string.Empty;
                string testFileName = Path.Combine(
                    testDir,
                    "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                File.Move(tempFile, testFileName);

                // 期待する日時
                var expectedDate = new DateTime(2022, 8, 31, 21, 54, 39, 227);

                // When: Exif撮影日時を更新
                bool result = FileTimestampUpdater.UpdateExifDate(testFileName);

                // Then: 更新が成功すること
                Assert.True(result);
                
                // Exifデータの検証
                using (var image = Image.Load(testFileName))
                {
                    var exifProfile = image.Metadata.ExifProfile;
                    Assert.NotNull(exifProfile);
                    
                    // Exifタグの値を取得
                    if (!exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeTag))
                    {
                        Assert.True(false, "DateTimeOriginal tag not found in EXIF data");
                        return;
                    }

                    // タグの値を安全に取得
                    var dateTimeValue = dateTimeTag.GetValue();
                    if (dateTimeValue == null)
                    {
                        Assert.True(false, "DateTimeOriginal tag value is null");
                        return;
                    }

                    // 文字列に変換して日時にパース
                    var dateTimeStr = dateTimeValue.ToString();
                    if (string.IsNullOrEmpty(dateTimeStr))
                    {
                        Assert.True(false, "DateTimeOriginal string value is null or empty");
                        return;
                    }

                    var actualDate = DateTime.ParseExact(
                        dateTimeStr,
                        "yyyy:MM:dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);

                    // 日時の検証
                    Assert.Equal(expectedDate.Year, actualDate.Year);
                    Assert.Equal(expectedDate.Month, actualDate.Month);
                    Assert.Equal(expectedDate.Day, actualDate.Day);
                    Assert.Equal(expectedDate.Hour, actualDate.Hour);
                    Assert.Equal(expectedDate.Minute, actualDate.Minute);
                    Assert.Equal(expectedDate.Second, actualDate.Second);
                }
            }
            finally
            {
                // テスト用ファイルの後片付け
                if (File.Exists(tempFile)) File.Delete(tempFile);
                string testFile = Path.Combine(
                    Path.GetTempPath(),
                    "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }
    }
}