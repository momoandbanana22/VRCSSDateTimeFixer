using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using VRCSSDateTimeFixer;
using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    public class ProgramTests
    {
        private static string CreateUniqueTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string CreateTestPng(string directory)
        {
            string path = Path.Combine(directory, $"{Guid.NewGuid()}.png");
            using var image = new Image<Rgba32>(100, 100);
            image[0,0] = new Rgba32(255,0,0,255);
            image.SaveAsPng(path);
            return path;
        }

        [Fact]
        public async Task ProcessFileAsync_UpdatesFileTimestampsAndExifBasedOnFileName()
        {
            // Arrange
            string dir = CreateUniqueTempDir();
            string src = CreateTestPng(dir);

            // Set initial timestamps intentionally different
            var initialCreation = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var initialWrite = new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Local);
            File.SetCreationTime(src, initialCreation);
            File.SetLastWriteTime(src, initialWrite);

            // Move to a valid VRChat filename that encodes the target datetime
            // Example: VRChat_1920x1080_2022-08-31_21-54-39.227.png
            string targetName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
            string dst = Path.Combine(dir, targetName);
            File.Move(src, dst);

            // 期待値（秒精度で比較）
            var expected = new DateTime(2022, 8, 31, 21, 54, 39, DateTimeKind.Local);

            // Act (suppress console output to avoid disposed writer issues under test runner)
            var originalOut = Console.Out;
            var originalErr = Console.Error;
            try
            {
                Console.SetOut(new StringWriter());
                Console.SetError(new StringWriter());
                await Program.ProcessFileAsync(dst);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }

            // Assert: File timestamps should be updated to the time encoded in file name (to seconds precision)
            var creation = File.GetCreationTime(dst);
            var write = File.GetLastWriteTime(dst);

            Assert.Equal(expected, new DateTime(creation.Year, creation.Month, creation.Day, creation.Hour, creation.Minute, creation.Second, creation.Kind));
            Assert.Equal(expected, new DateTime(write.Year, write.Month, write.Day, write.Hour, write.Minute, write.Second, write.Kind));

            // Assert: Exif DateTimeOriginal should be updated
            using (var image = Image.Load(dst))
            {
                var exif = image.Metadata.ExifProfile;
                Assert.NotNull(exif);
                if (exif!.TryGetValue(ExifTag.DateTimeOriginal, out var dto))
                {
                    var dateTimeOriginal = dto?.ToString();
                    Assert.False(string.IsNullOrEmpty(dateTimeOriginal));
                    // Exif tag stores format "yyyy:MM:dd HH:mm:ss"
                    Assert.StartsWith("2022:08:31 21:54:39", dateTimeOriginal);
                }
                else
                {
                    Assert.True(false, "Exif DateTimeOriginal not found");
                }
            }
        }
    }
}
