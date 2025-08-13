using System;
using System.IO;
using VRCSSDateTimeFixer.Services;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class VRChatScreenshotProcessorTests : IDisposable
    {
        // テストデータ
        private const string TestFileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
        private const string NonExistentFileName = "nonexistent_file.png";
        
        private readonly string _testDir;
        private readonly string _testFilePath;
        private readonly string _nonExistentFile;

        public VRChatScreenshotProcessorTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "VRChatScreenshotProcessorTests");
            Directory.CreateDirectory(_testDir);
            
            _testFilePath = Path.Combine(_testDir, TestFileName);
            File.WriteAllText(_testFilePath, "test");
            
            _nonExistentFile = Path.Combine(_testDir, NonExistentFileName);
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

        #region 正常系テスト

        // 正常系のテストは後で実装

        #endregion

        #region 異常系テスト

        [Theory]
        [InlineData(null, "ファイルパスが指定されていません")]
        [InlineData("", "ファイルパスが指定されていません")]
        [InlineData("nonexistent_file.png", "ファイルが見つかりません")]
        public void 不正なファイルパスを指定した場合_失敗結果を返すこと(string? filePath, string expectedErrorMessage)
        {
            // Given: 不正なファイルパス
            var targetPath = filePath == "nonexistent_file.png" 
                ? _nonExistentFile 
                : filePath;

            // When: 処理を実行
            var result = VRChatScreenshotProcessor.ProcessFile(targetPath!);

            // Then: 失敗結果が返り、適切なエラーメッセージが含まれていること
            AssertProcessResult(
                result: result,
                expectedSuccess: false,
                expectedMessage: expectedErrorMessage);
        }

        #endregion

        #region プライベートヘルパーメソッド

        private static void AssertProcessResult(
            ProcessResult result,
            bool expectedSuccess,
            string? expectedMessage = null,
            DateTime? expectedDateTime = null,
            bool? expectedTimestampUpdated = null,
            bool? expectedExifUpdated = null)
        {
            Assert.NotNull(result);
            Assert.Equal(expectedSuccess, result.Success);
            
            if (expectedMessage != null)
            {
                Assert.Contains(expectedMessage, result.Message);
            }

            if (expectedDateTime.HasValue)
            {
                Assert.NotNull(result.ExtractedDateTime);
                Assert.Equal(expectedDateTime.Value, result.ExtractedDateTime!.Value);
            }
            else
            {
                Assert.Null(result.ExtractedDateTime);
            }

            if (expectedTimestampUpdated.HasValue)
            {
                Assert.Equal(expectedTimestampUpdated.Value, result.TimestampUpdated);
            }

            if (expectedExifUpdated.HasValue)
            {
                Assert.Equal(expectedExifUpdated.Value, result.ExifUpdated);
            }
        }

        #endregion
    }
}
