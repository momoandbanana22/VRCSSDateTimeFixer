using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

using VRCSSDateTimeFixer.Services;

using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class ExifMetadataUpdaterTests : IDisposable
    {
        private readonly string _testImagePath;
        private readonly string _tempImagePath;
        private readonly DateTime _testDateTime = new DateTime(2023, 1, 15, 12, 30, 0);

        public ExifMetadataUpdaterTests()
        {
            // テスト用の一時的なPNGファイルを作成
            _testImagePath = Path.Combine(Path.GetTempPath(), "test_image.png");
            _tempImagePath = Path.GetTempFileName();

            // テスト用の空の画像ファイルを作成
            using (var image = new Image<Rgba32>(100, 100))
            {
                image.Save(_testImagePath);
            }
        }

        public void Dispose()
        {
            // テスト用の一時ファイルを削除
            if (File.Exists(_testImagePath)) File.Delete(_testImagePath);
            if (File.Exists(_tempImagePath)) File.Delete(_tempImagePath);
        }

        [Fact]
        public async Task UpdateExifMetadataAsync_ShouldUpdateDateTimeOriginal()
        {
            // Arrange
            var updater = new ExifMetadataUpdater();

            // Act
            var result = await updater.UpdateExifMetadataAsync(_testImagePath, _testDateTime);

            // Assert
            Assert.True(result);

            // EXIFデータを確認
            using (var image = await Image.LoadAsync(_testImagePath))
            {
                var exifProfile = image.Metadata.ExifProfile;
                Assert.NotNull(exifProfile);

                Assert.True(exifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeTag),
                    "DateTimeOriginal tag not found in EXIF data");

                var dateTimeValue = dateTimeTag?.GetValue() ??
                    throw new InvalidOperationException("DateTimeOriginal tag value is null");

                var dateTimeStr = dateTimeValue.ToString() ??
                    throw new InvalidOperationException("DateTimeOriginal string value is null or empty");

                Assert.Equal(_testDateTime.ToString("yyyy:MM:dd HH:mm:ss"), dateTimeStr);
            }
        }

        [Fact]
        public async Task UpdateExifMetadataAsync_WithInvalidPath_ShouldReturnFalse()
        {
            // Arrange
            var updater = new ExifMetadataUpdater();
            var invalidPath = Path.Combine(Path.GetTempPath(), "nonexistent_image.png");

            // Act
            var result = await updater.UpdateExifMetadataAsync(invalidPath, _testDateTime);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateExifMetadataAsync_WithReadOnlyFile_ShouldReturnFalse()
        {
            // Arrange
            var updater = new ExifMetadataUpdater();
            File.SetAttributes(_testImagePath, FileAttributes.ReadOnly);

            try
            {
                // Act
                var result = await updater.UpdateExifMetadataAsync(_testImagePath, _testDateTime);

                // Assert
                Assert.False(result);
            }
            finally
            {
                // 後処理で読み取り専用属性を解除
                File.SetAttributes(_testImagePath, FileAttributes.Normal);
            }
        }
    }
}
