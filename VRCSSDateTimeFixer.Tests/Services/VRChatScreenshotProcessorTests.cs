using System;
using System.IO;
using VRCSSDateTimeFixer.Services;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class VRChatScreenshotProcessorTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _testFilePath;
        private readonly string _nonExistentFile;

        public VRChatScreenshotProcessorTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "VRChatScreenshotProcessorTests");
            Directory.CreateDirectory(_testDir);
            
            _testFilePath = Path.Combine(_testDir, "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
            File.WriteAllText(_testFilePath, "test");
            
            _nonExistentFile = Path.Combine(_testDir, "nonexistent_file.png");
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                try
                {
                    Directory.Delete(_testDir, true);
                }
                catch
                {
                    // 削除に失敗しても無視
                }
            }
        }

        [Fact]
        public void 存在しないファイルを指定した場合_失敗結果を返すこと()
        {
            // Given: 存在しないファイルパス
            string nonExistentFile = _nonExistentFile;

            // When: 処理を実行
            var result = VRChatScreenshotProcessor.ProcessFile(nonExistentFile);

            // Then: 失敗結果が返り、適切なエラーメッセージが含まれていること
            Assert.False(result.Success);
            Assert.Contains("ファイルが見つかりません", result.Message);
            Assert.Null(result.ExtractedDateTime);
            Assert.False(result.TimestampUpdated);
            Assert.False(result.ExifUpdated);
        }

        [Fact]
        public void 空のファイルパスを指定した場合_失敗結果を返すこと()
        {
            // Given: 空のファイルパス
            string emptyPath = string.Empty;

            // When: 処理を実行
            var result = VRChatScreenshotProcessor.ProcessFile(emptyPath);

            // Then: 失敗結果が返り、適切なエラーメッセージが含まれていること
            Assert.False(result.Success);
            Assert.Contains("ファイルパスが指定されていません", result.Message);
            Assert.Null(result.ExtractedDateTime);
            Assert.False(result.TimestampUpdated);
            Assert.False(result.ExifUpdated);
        }

        [Fact]
        public void nullを指定した場合_失敗結果を返すこと()
        {
            // Given: nullのファイルパス
            string nullPath = null;

            // When: 処理を実行
            var result = VRChatScreenshotProcessor.ProcessFile(nullPath);

            // Then: 失敗結果が返り、適切なエラーメッセージが含まれていること
            Assert.False(result.Success);
            Assert.Contains("ファイルパスが指定されていません", result.Message);
            Assert.Null(result.ExtractedDateTime);
            Assert.False(result.TimestampUpdated);
            Assert.False(result.ExifUpdated);
        }
    }
}
