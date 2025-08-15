using VRCSSDateTimeFixer.Services;

using Xunit;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class VRChatScreenshotProcessorErrorHandlingTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _nonExistentFile;
        private readonly string _readOnlyFile;
        private readonly string _unsupportedFile;
        private readonly string _invalidFormatFile;

        public VRChatScreenshotProcessorErrorHandlingTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "VRChatScreenshotProcessorErrorHandlingTests");
            Directory.CreateDirectory(_testDir);

            // テスト用のファイルパスを設定
            _nonExistentFile = Path.Combine(_testDir, "nonexistent_file.png");

            // 読み取り専用ファイルを作成
            _readOnlyFile = Path.Combine(_testDir, "readonly.png");
            File.WriteAllText(_readOnlyFile, "test");
            File.SetAttributes(_readOnlyFile, FileAttributes.ReadOnly);

            // サポートされていない形式のファイルを作成
            _unsupportedFile = Path.Combine(_testDir, "unsupported.txt");
            File.WriteAllText(_unsupportedFile, "unsupported");

            // 不正な形式のファイル名のファイルを作成
            _invalidFormatFile = Path.Combine(_testDir, "invalid_format.png");
            File.WriteAllText(_invalidFormatFile, "invalid format");
        }

        public void Dispose()
        {
            // 読み取り専用属性を削除
            if (File.Exists(_readOnlyFile))
            {
                File.SetAttributes(_readOnlyFile, FileAttributes.Normal);
            }

            // テストファイルを削除
            foreach (var file in new[] { _readOnlyFile, _unsupportedFile, _invalidFormatFile })
            {
                try
                {
                    if (File.Exists(file)) File.Delete(file);
                }
                catch
                {
                    // 削除に失敗しても無視
                }
            }

            // テストディレクトリを削除
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
        public async Task ProcessFileAsync_WhenFileDoesNotExist_ReturnsError()
        {
            // Act
            var result = await VRChatScreenshotProcessor.ProcessFileAsync(_nonExistentFile);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("ファイルが見つかりません", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenFileIsReadOnly_ReturnsError()
        {
            // Act
            var result = await VRChatScreenshotProcessor.ProcessFileAsync(_readOnlyFile);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("読み取り専用ファイル", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenFileFormatIsUnsupported_ReturnsError()
        {
            // Act
            var result = await VRChatScreenshotProcessor.ProcessFileAsync(_unsupportedFile);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("サポートされていないファイル形式", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenFileNameHasInvalidFormat_ReturnsError()
        {
            // Act
            var result = await VRChatScreenshotProcessor.ProcessFileAsync(_invalidFormatFile);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("ファイル名から日時を抽出できません", result.ErrorMessage);
        }
    }
}
