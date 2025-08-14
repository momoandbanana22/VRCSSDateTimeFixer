using System;
using System.IO;
using System.Text;
using Xunit;
using VRCSSDateTimeFixer.Services;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class ProgressDisplayIntegrationTests : IDisposable
    {
        private ProgressDisplay _progressDisplay = null!;
        private StringWriter _outputWriter = null!;
        private StringWriter _errorWriter = null!;
        private TextWriter _originalOutput = null!;
        private TextWriter _originalError = null!;
        private bool _disposed;

        public ProgressDisplayIntegrationTests()
        {
            ResetProgressDisplay();
        }

        private void ResetProgressDisplay()
        {
            _progressDisplay?.Dispose();
            _outputWriter?.Dispose();
            _errorWriter?.Dispose();
            
            _outputWriter = new StringWriter();
            _errorWriter = new StringWriter();
            _originalOutput = Console.Out;
            _originalError = Console.Error;
            
            Console.SetOut(_outputWriter);
            Console.SetError(_errorWriter);
            
            _progressDisplay = new ProgressDisplay();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _progressDisplay.Dispose();
                    _outputWriter.Dispose();
                    _errorWriter.Dispose();
                    Console.SetOut(_originalOutput);
                    Console.SetError(_originalError);
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ClearOutput()
        {
            _outputWriter.GetStringBuilder().Clear();
        }

        [Fact]
        public void 完全な処理フロー_正常系_正しい形式で表示されること()
        {
            // Arrange
            string fileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
            var testDate = new DateTime(2022, 8, 31, 21, 54, 39, 227);
            
            // Act
            _progressDisplay.StartProcessing(fileName);
            _progressDisplay.ShowExtractedDateTime(testDate);
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            _progressDisplay.ShowExifUpdateResult(true);

            // Assert
            var output = _outputWriter.ToString().Trim();
            var expectedOutput = $"{fileName}:2022年08月31日 21時54分39.227 作成日時：更新済 更新日時：更新済 撮影日時：更新済";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void 更新失敗時の表示_正しく表示されること()
        {
            // Arrange
            var testDate = new DateTime(2023, 1, 1, 12, 0, 0);
            string fileName = "test.png";
            
            // Act
            _progressDisplay.StartProcessing(fileName);
            _progressDisplay.ShowExtractedDateTime(testDate);
            _progressDisplay.ShowCreationTimeUpdateResult(false);
            _progressDisplay.ShowLastWriteTimeUpdateResult(false);
            _progressDisplay.ShowExifUpdateResult(false);

            // Assert
            var output = _outputWriter.ToString().Trim();
            var expectedOutput = $"{fileName}:2023年01月01日 12時00分00.000 作成日時：スキップ 更新日時：スキップ 撮影日時：スキップ";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void エラーメッセージが正しく表示されること()
        {
            // Arrange
            string errorMessage = "ファイルが存在しません";
            
            // Act
            _progressDisplay.ShowError(errorMessage);

            // Assert
            var errorOutput = _errorWriter.ToString().Trim();
            Assert.Equal($"エラー: {errorMessage}", errorOutput);
        }

        [Fact]
        public void 表示順序_メソッド呼び出し順に表示されること()
        {
            // Arrange
            var testDate1 = new DateTime(2022, 8, 31, 21, 54, 39, 227);
            var testDate2 = new DateTime(2022, 9, 1, 12, 34, 56, 789);
            
            // Act
            _progressDisplay.StartProcessing("test1.png");
            _progressDisplay.ShowExtractedDateTime(testDate1);
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            _progressDisplay.ShowExifUpdateResult(true);
            
            _progressDisplay.StartProcessing("test2.png");
            _progressDisplay.ShowExtractedDateTime(testDate2);
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            _progressDisplay.ShowExifUpdateResult(true);
            
            // Assert
            var output = _outputWriter.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, output.Length);
            Assert.Contains("test1.png:", output[0]);
            Assert.Contains("test2.png:", output[1]);
        }
    }
}
