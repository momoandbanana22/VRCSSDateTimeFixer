using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    public class FileTimestampUpdaterTests : IDisposable
    {
        // テストデータ
        private const string TestImageName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
        private static readonly DateTime ExpectedDate = new(2022, 8, 31, 21, 54, 39, 227);
        private readonly List<string> _tempFiles = new();

        public void Dispose()
        {
            // テストで作成した一時ファイルを削除
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file)) File.Delete(file);
                }
                catch
                {
                    // ファイル削除に失敗しても無視
                }
            }
            _tempFiles.Clear();
        }

        [Fact]
        public void 有効なファイル名から日時を抽出してタイムスタンプを更新できること()
        {
            // Arrange
            string testFile = CreateTestFile("test.txt");
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            bool result = FileTimestampUpdater.UpdateFileTimestamp(testImageFile);

            // Assert
            Assert.True(result);
            var fileInfo = new FileInfo(testImageFile);
            AssertFileTimestampsMatchExpected(fileInfo);
        }

        [Fact]
        public void 有効なPNGファイルのExif撮影日時を更新できること()
        {
            // Arrange
            string testFile = CreateTestPngFile();
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            bool result = FileTimestampUpdater.UpdateExifDate(testImageFile);

            // Assert
            Assert.True(result);
            AssertExifDateMatchesExpected(testImageFile);
        }

        #region プライベートヘルパーメソッド

        private string CreateTestFile(string extension = ".tmp")
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            File.WriteAllText(tempFile, "test");
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        private string CreateTestPngFile()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            using var image = new Image<Rgba32>(100, 100);
            image.SaveAsPng(tempFile);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        private string MoveToTestImageFile(string sourceFile)
        {
            string targetFile = Path.Combine(
                Path.GetDirectoryName(sourceFile) ?? string.Empty,
                TestImageName);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            File.Move(sourceFile, targetFile);
            _tempFiles.Add(targetFile);
            return targetFile;
        }

        private static void AssertFileTimestampsMatchExpected(FileSystemInfo fileInfo)
        {
            Assert.Equal(ExpectedDate, fileInfo.CreationTime);
            Assert.Equal(ExpectedDate, fileInfo.LastWriteTime);
        }

        private static void AssertExifDateMatchesExpected(string imagePath)
        {
            using var image = Image.Load(imagePath);
            var exifProfile = image.Metadata.ExifProfile;

            Assert.NotNull(exifProfile);
            Assert.True(exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeTag),
                "DateTimeOriginal tag not found in EXIF data");

            var dateTimeValue = dateTimeTag?.GetValue() ??
                throw new InvalidOperationException("DateTimeOriginal tag value is null");

            var dateTimeStr = dateTimeValue.ToString() ??
                throw new InvalidOperationException("DateTimeOriginal string value is null or empty");

            var actualDate = DateTime.ParseExact(
                dateTimeStr,
                "yyyy:MM:dd HH:mm:ss",
                CultureInfo.InvariantCulture);

            Assert.Equal(ExpectedDate.Year, actualDate.Year);
            Assert.Equal(ExpectedDate.Month, actualDate.Month);
            Assert.Equal(ExpectedDate.Day, actualDate.Day);
            Assert.Equal(ExpectedDate.Hour, actualDate.Hour);
            Assert.Equal(ExpectedDate.Minute, actualDate.Minute);
            Assert.Equal(ExpectedDate.Second, actualDate.Second);
        }

        #endregion
    }
}