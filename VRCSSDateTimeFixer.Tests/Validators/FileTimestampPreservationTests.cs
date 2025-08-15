using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using VRCSSDateTimeFixer.Services;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer.Tests.Validators
{
    public class FileTimestampPreservationTests : IDisposable
    {
        private const string TestImageName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
        private readonly List<string> _tempFiles = new();
        private static readonly DateTime ExpectedDate = new(2022, 8, 31, 21, 54, 39, 227);

        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file)) 
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                }
                catch { /* Ignore cleanup errors */ }
            }
            _tempFiles.Clear();
        }

        [Fact]
        public async Task UpdateExifDateAsync_ShouldNotModifyFileTimestamps()
        {
            try
            {
                // Arrange - Create a proper PNG file with Exif data
                string testImageFile = CreateTestPngFileWithExif();
                Console.WriteLine($"[TEST] Created test file at: {testImageFile}");
                
                // Set initial timestamps
                var initialCreationTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
                var initialLastWriteTime = new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Local);
                
                File.SetCreationTime(testImageFile, initialCreationTime);
                File.SetLastWriteTime(testImageFile, initialLastWriteTime);
                
                Console.WriteLine($"[TEST] Set initial timestamps - Creation: {initialCreationTime}, LastWrite: {initialLastWriteTime}");
                
                // Ensure the file has a valid VRChat screenshot name for date extraction
                string directory = Path.GetDirectoryName(testImageFile) ?? string.Empty;
                string validName = Path.Combine(directory, TestImageName);
                
                if (File.Exists(validName))
                {
                    File.Delete(validName);
                }
                
                File.Move(testImageFile, validName);
                testImageFile = validName;
                _tempFiles.Add(testImageFile);
                
                Console.WriteLine($"[TEST] Moved to valid filename: {testImageFile}");
                Console.WriteLine($"[TEST] File exists: {File.Exists(testImageFile)}");
                
                // Verify the file has the expected name format
                string fileName = Path.GetFileName(testImageFile);
                Console.WriteLine($"[TEST] Verifying file name format: {fileName}");
                Assert.StartsWith("VRChat_", fileName);
                
                // Verify the file has content
                var fileInfo = new FileInfo(testImageFile);
                Console.WriteLine($"[TEST] File size: {fileInfo.Length} bytes");
                Assert.True(fileInfo.Length > 0, "Test file is empty");
                
                // Verify Exif data exists before update
                using (var image = await Task.Run(() => Image.Load(testImageFile)))
                {
                    bool hasExif = image.Metadata.ExifProfile != null;
                    Console.WriteLine($"[TEST] File has Exif data before update: {hasExif}");
                    Assert.True(hasExif, "Test file should have Exif data before update");
                }
                
                // Act - Update Exif data
                Console.WriteLine("[TEST] Calling UpdateExifDateAsync...");
                bool result = await FileTimestampUpdater.UpdateExifDateAsync(testImageFile);
                Console.WriteLine($"[TEST] UpdateExifDateAsync result: {result}");
                
                // Verify Exif data after update
                using (var image = await Task.Run(() => Image.Load(testImageFile)))
                {
                    var exif = image.Metadata.ExifProfile;
                    bool hasExif = exif != null;
                    Console.WriteLine($"[TEST] File has Exif data after update: {hasExif}");
                    if (exif?.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeOriginal) == true)
                    {
                        Console.WriteLine($"[TEST] DateTimeOriginal after update: {dateTimeOriginal?.ToString()}");
                    }
                    else
                    {
                        Console.WriteLine("[TEST] Could not read DateTimeOriginal from Exif data");
                    }
                }
                
                // Get timestamps after update
                var afterCreationTime = File.GetCreationTime(testImageFile);
                var afterLastWriteTime = File.GetLastWriteTime(testImageFile);
                
                Console.WriteLine($"[TEST] Timestamps after update - Creation: {afterCreationTime}, LastWrite: {afterLastWriteTime}");
                
                // Assert
                Assert.True(result, "Exif update should succeed. Make sure the test file contains valid Exif data.");
                
                // Verify timestamps remain unchanged
                Assert.Equal(initialCreationTime, afterCreationTime);
                Assert.Equal(initialLastWriteTime, afterLastWriteTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEST ERROR] {ex}");
                throw;
            }
        }
        
        private string CreateTestPngFileWithExif()
        {
            string uniqueDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(uniqueDir);
            string tempFile = Path.Combine(uniqueDir, $"{Guid.NewGuid()}.png");
            
            // Create a new image with Exif data
            using (var image = new Image<Rgba32>(100, 100))
            {
                // Add Exif data
                image.Metadata.ExifProfile ??= new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile();
                
                // Add some test text to the image to ensure it's not empty
                image[10, 10] = new Rgba32(255, 0, 0, 255); // Red pixel
                
                // Set multiple Exif tags to ensure the profile is valid
                var now = DateTime.Now;
                image.Metadata.ExifProfile.SetValue(ExifTag.DateTimeOriginal, now.ToString("yyyy:MM:dd HH:mm:ss"));
                image.Metadata.ExifProfile.SetValue(ExifTag.Make, "Test");
                image.Metadata.ExifProfile.SetValue(ExifTag.Model, "Test Image");
                
                // Save the image with high compression to ensure it's written correctly
                var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder()
                {
                    ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.RgbWithAlpha,
                    CompressionLevel = SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.BestCompression
                };
                
                // Save the image
                using (var stream = File.Create(tempFile))
                {
                    image.SaveAsPng(stream, encoder);
                }
                
                Console.WriteLine($"Created test image with Exif data at: {tempFile}");
                Console.WriteLine($"Image size: {new FileInfo(tempFile).Length} bytes");
                
                // Verify the file was created and has content
                if (!File.Exists(tempFile) || new FileInfo(tempFile).Length == 0)
                {
                    throw new InvalidOperationException("Failed to create test image file");
                }
                
                // Verify the Exif data was written
                using (var verifyImage = Image.Load(tempFile))
                {
                    if (verifyImage.Metadata.ExifProfile == null)
                    {
                        throw new InvalidOperationException("Failed to create Exif profile in test image");
                    }
                    
                    Console.WriteLine("Successfully verified Exif data in test image");
                }
            }
            
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        

        private string CreateTestFile(string extension = ".tmp")
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            File.WriteAllText(tempFile, "test");
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        private string MoveToTestImageFile(string sourceFile)
        {
            string targetFile = Path.Combine(
                Path.GetDirectoryName(sourceFile) ?? string.Empty,
                TestImageName);

            if (File.Exists(targetFile))
            {
                try
                {
                    File.SetAttributes(targetFile, FileAttributes.Normal);
                    File.Delete(targetFile);
                }
                catch (IOException)
                {
                    string directory = Path.GetDirectoryName(targetFile) ?? string.Empty;
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(targetFile);
                    string extension = Path.GetExtension(targetFile);
                    targetFile = Path.Combine(directory, $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}");
                }
            }

            File.Copy(sourceFile, targetFile, true);
            try { File.Delete(sourceFile); } catch { }

            _tempFiles.Add(targetFile);
            return targetFile;
        }
    }
}
