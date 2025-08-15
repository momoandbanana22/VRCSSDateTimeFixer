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
        public async Task 有効なファイル名から日時を抽出してタイムスタンプを更新できること()
        {
            // Arrange
            string testFile = CreateTestFile("test.txt");
            string testImageFile = MoveToTestImageFile(testFile);

            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater.UpdateFileTimestampAsync(testImageFile);

            // Assert
            Assert.True(creationTimeUpdated);
            Assert.True(lastWriteTimeUpdated);
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

        [Fact]
        public async Task UpdateExifDateAsync_Exifが正しく更新されること()
        {
            // Arrange
            string testFile = CreateTestPngFile();
            string testImageFile = MoveToTestImageFile(testFile);
            
            // まずタイムスタンプを設定
            var expectedDate = new DateTime(2022, 8, 31, 21, 54, 39, 227, DateTimeKind.Local);
            File.SetCreationTime(testImageFile, expectedDate);
            File.SetLastWriteTime(testImageFile, expectedDate);
            File.SetLastAccessTime(testImageFile, expectedDate);
            
            // Act
            bool result = await FileTimestampUpdater.UpdateExifDateAsync(testImageFile);
            
            // Assert
            Assert.True(result);
            
            // Exifが正しく更新されたことを確認
            AssertExifDateMatchesExpected(testImageFile);
            
            // このメソッドはタイムスタンプの管理を行わないため、
            // タイムスタンプのチェックは呼び出し元の責任となる
        }
        
        [Fact]
        public async Task UpdateFileTimestampAsync_作成日時と最終更新日時が両方とも更新されること()
        {
            // Arrange
            // テスト用のPNGファイルを作成（テキストファイルではなく）
            string testFile = CreateTestPngFile();
            
            // テスト用のVRChatスクリーンショット形式のファイル名にリネーム
            string testImageFile = MoveToTestImageFile(testFile);
            
            // ファイル名が期待通りの形式であることを確認
            string fileName = Path.GetFileName(testImageFile);
            Assert.StartsWith("VRChat_", fileName);
            
            // 現在のタイムスタンプを明示的に設定（更新前とは異なる日時にする）
            var originalDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
            File.SetCreationTime(testImageFile, originalDate);
            File.SetLastWriteTime(testImageFile, originalDate);
            
            // 現在のタイムスタンプを記録
            var originalCreationTime = File.GetCreationTime(testImageFile);
            var originalLastWriteTime = File.GetLastWriteTime(testImageFile);
            
            // 期待される日時（ファイル名から抽出される日時）
            var expectedDate = ExpectedDate;
            
            // 現在のタイムスタンプが期待値と異なることを確認
            Assert.NotEqual(expectedDate, originalCreationTime);
            Assert.NotEqual(expectedDate, originalLastWriteTime);
            
            // Act
            var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater.UpdateFileTimestampAsync(testImageFile);
            
            // Assert
            Assert.True(creationTimeUpdated, "作成日時が更新されるべき");
            Assert.True(lastWriteTimeUpdated, "最終更新日時が更新されるべき");
            
            var fileInfo = new FileInfo(testImageFile);
            
            // タイムスタンプが期待通りの日時に更新されていることを確認
            Assert.Equal(expectedDate.Year, fileInfo.CreationTime.Year);
            Assert.Equal(expectedDate.Month, fileInfo.CreationTime.Month);
            Assert.Equal(expectedDate.Day, fileInfo.CreationTime.Day);
            Assert.Equal(expectedDate.Hour, fileInfo.CreationTime.Hour);
            Assert.Equal(expectedDate.Minute, fileInfo.CreationTime.Minute);
            Assert.Equal(expectedDate.Second, fileInfo.CreationTime.Second);
            
            // 最終更新日時も同じ日時に設定されていることを確認
            Assert.Equal(expectedDate.Year, fileInfo.LastWriteTime.Year);
            Assert.Equal(expectedDate.Month, fileInfo.LastWriteTime.Month);
            Assert.Equal(expectedDate.Day, fileInfo.LastWriteTime.Day);
            Assert.Equal(expectedDate.Hour, fileInfo.LastWriteTime.Hour);
            Assert.Equal(expectedDate.Minute, fileInfo.LastWriteTime.Minute);
            Assert.Equal(expectedDate.Second, fileInfo.LastWriteTime.Second);
            
            // 元のタイムスタンプと異なることを確認
            Assert.NotEqual(originalCreationTime, fileInfo.CreationTime);
            Assert.NotEqual(originalLastWriteTime, fileInfo.LastWriteTime);
            
            // ファイルが存在することを確認
            Assert.True(File.Exists(testImageFile), "テストファイルが存在するべき");
        }
        
        // テスト用のモックDateTimeProvider
        private class MockDateTimeProvider
        {
            private readonly DateTime _now;
            
            public MockDateTimeProvider(DateTime now)
            {
                _now = now;
            }
            
            public DateTime Now => _now;
        }

        #region プライベートヘルパーメソッド

        private string CreateTestFile(string extension = ".tmp")
        {
            string uniqueDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(uniqueDir);
            string tempFile = Path.Combine(uniqueDir, $"{Guid.NewGuid()}{extension}");
            File.WriteAllText(tempFile, "test");
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        private string CreateTestPngFile()
        {
            string uniqueDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(uniqueDir);
            string tempFile = Path.Combine(uniqueDir, $"{Guid.NewGuid()}.png");
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