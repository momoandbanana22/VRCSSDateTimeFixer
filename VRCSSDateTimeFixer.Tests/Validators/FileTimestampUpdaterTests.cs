using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Validators
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
        public async Task 有効なPNGファイルのExif撮影日時を更新できること()
        {
            // Arrange
            string testFile = CreateTestPngFile();
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            bool result = await FileTimestampUpdater.UpdateExifDateAsync(testImageFile);

            // Assert
            Assert.True(result);
            AssertExifDateMatchesExpected(testImageFile);
        }

        [Fact]
        public async Task UpdateFileTimestampAsync_有効なファイル名から日時を抽出してタイムスタンプを更新できること()
        {
            // Arrange
            string testFile = CreateTestFile("test.txt");
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater
                .UpdateFileTimestampAsync(testImageFile);

            // Assert
            Assert.True(creationTimeUpdated);
            Assert.True(lastWriteTimeUpdated);
            var fileInfo = new FileInfo(testImageFile);
            AssertFileTimestampsMatchExpected(fileInfo);
        }

        [Fact]
        public async Task UpdateFileTimestampAsync_無効なファイルパスではfalseを返すこと()
        {
            // Arrange
            string invalidPath = Path.Combine(Path.GetTempPath(), "nonexistent.txt");

            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater
                .UpdateFileTimestampAsync(invalidPath);

            // Assert
            Assert.False(creationTimeUpdated);
            Assert.False(lastWriteTimeUpdated);
        }

        [Fact]
        public async Task UpdateExifDateAsync_有効なPNGファイルのExif撮影日時を更新できること()
        {
            // Arrange
            string testFile = CreateTestPngFile();
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            bool result = await FileTimestampUpdater
                .UpdateExifDateAsync(testImageFile);

            // Assert
            Assert.True(result);
            AssertExifDateMatchesExpected(testImageFile);
        }

        [Fact]
        public async Task UpdateFileTimestampAsync_読み取り専用ファイルのタイムスタンプを更新できること()
        {
            // Arrange
            string testFile = CreateTestFile("test.txt");
            string testImageFile = MoveToTestImageFile(testFile);
            File.SetAttributes(testImageFile, FileAttributes.ReadOnly);

            try
            {
                // Act
                var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater
                    .UpdateFileTimestampAsync(testImageFile);

                // Assert
                Assert.True(creationTimeUpdated);
                Assert.True(lastWriteTimeUpdated);
                var fileInfo = new FileInfo(testImageFile);
                AssertFileTimestampsMatchExpected(fileInfo);
            }
            finally
            {
                // テスト後に読み取り専用属性を解除
                File.SetAttributes(testImageFile, FileAttributes.Normal);
            }
        }

        [Fact]
        public async Task UpdateFileTimestampAsync_無効なファイル名形式の場合はfalseを返すこと()
        {
            // Arrange
            string invalidFileName = "invalid_filename.txt";
            string testFile = CreateTestFile(invalidFileName);

            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater
                .UpdateFileTimestampAsync(testFile);

            // Assert
            Assert.False(creationTimeUpdated);
            Assert.False(lastWriteTimeUpdated);
        }

        [Fact]
        public async Task UpdateFileTimestampAsync_異なる形式のファイル名でも正しく日時を抽出できること()
        {
            // Arrange
            string testFile = CreateTestFile("VRChat_2022-08-31_21-54-39.227_1920x1080.png");
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater
                .UpdateFileTimestampAsync(testImageFile);

            // Assert
            Assert.True(creationTimeUpdated);
            Assert.True(lastWriteTimeUpdated);
            var fileInfo = new FileInfo(testImageFile);
            AssertFileTimestampsMatchExpected(fileInfo);
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

            // 既存のファイルがロックされていないことを確認して削除
            if (File.Exists(targetFile))
            {
                try
                {
                    File.SetAttributes(targetFile, FileAttributes.Normal);
                    File.Delete(targetFile);
                }
                catch (IOException)
                {
                    // 削除に失敗した場合は別の一意なファイル名を生成
                    string directory = Path.GetDirectoryName(targetFile) ?? string.Empty;
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(targetFile);
                    string extension = Path.GetExtension(targetFile);
                    targetFile = Path.Combine(directory, $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}");
                }
            }

            // ファイルをコピーしてから元のファイルを削除
            File.Copy(sourceFile, targetFile, true);
            try
            {
                File.Delete(sourceFile);
            }
            catch
            {
                // 元のファイルの削除に失敗しても無視
            }

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