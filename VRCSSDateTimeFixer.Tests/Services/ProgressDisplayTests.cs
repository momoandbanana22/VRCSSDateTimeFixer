using System;
using System.IO;
using Xunit;
using VRCSSDateTimeFixer.Services;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class ProgressDisplayTests : IDisposable
    {
        private StringWriter _outputWriter = null!;
        private StringWriter _errorWriter = null!;
        private ProgressDisplay _progressDisplay = null!;
        private bool _disposed;

        public ProgressDisplayTests()
        {
            ResetProgressDisplay();
        }

        private void ResetProgressDisplay()
        {
            _outputWriter?.Dispose();
            _errorWriter?.Dispose();
            
            _outputWriter = new StringWriter();
            _errorWriter = new StringWriter();
            
            _progressDisplay?.Dispose();
            _progressDisplay = new ProgressDisplay(_outputWriter, _errorWriter);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _progressDisplay?.Dispose();
                    _outputWriter?.Dispose();
                    _errorWriter?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string GetOutput() => _outputWriter.ToString();
        private string GetErrorOutput() => _errorWriter.ToString();

        [Fact]
        public void StartProcessing_ファイル名をバッファに保存する()
        {
            // Arrange
            string fileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
            
            // Act
            _progressDisplay.StartProcessing(fileName);
            
            // 出力はまだされていないはず
            var output = GetOutput();
            Assert.Equal(string.Empty, output);

            // 次の出力で確認
            _progressDisplay.ShowExifUpdateResult(true);
            output = GetOutput();
            
            // Assert
            Assert.StartsWith(fileName, output);
            Assert.Contains("撮影日時：更新済", output);
        }

        [Fact]
        public void ShowExtractedDateTime_日時をバッファに保存する()
        {
            // Arrange
            var dateTime = new DateTime(2022, 8, 31, 21, 54, 39, 227);
            string expected = ":2022年08月31日 21時54分39.227";
            
            // Act
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowExtractedDateTime(dateTime);
            
            // 出力はまだされていないはず
            var output = GetOutput();
            Assert.Equal(string.Empty, output);

            // 次の出力で確認
            _progressDisplay.ShowExifUpdateResult(true);
            output = GetOutput();
            
            // Assert
            Assert.Contains(expected, output);
        }

        [Fact]
        public void ShowCreationTimeUpdateResult_更新成功時に更新済みをバッファに保存する()
        {
            // Arrange
            string expected = "作成日時：更新済";
            
            // Act
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            
            // 出力はまだされていないはず
            var output = GetOutput();
            Assert.Equal(string.Empty, output);

            // 次の出力で確認
            _progressDisplay.ShowExifUpdateResult(true);
            output = GetOutput();
            
            // Assert
            Assert.Contains(expected, output);
        }

        [Fact]
        public void ShowLastWriteTimeUpdateResult_更新成功時に更新済みをバッファに保存する()
        {
            // Arrange
            string expected = "更新日時：更新済";
            
            // Act
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            
            // 出力はまだされていないはず
            var output = GetOutput();
            Assert.Equal(string.Empty, output);

            // 次の出力で確認
            _progressDisplay.ShowExifUpdateResult(true);
            output = GetOutput();
            
            // Assert
            Assert.Contains(expected, output);
        }

        [Fact]
        public void ShowExifUpdateResult_更新成功時にバッファの内容を出力する()
        {
            // Arrange
            string expected = "test:2022年08月31日 21時54分39.227 作成日時：更新済 更新日時：更新済 撮影日時：更新済" + Environment.NewLine;
            
            // Act
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowExtractedDateTime(new DateTime(2022, 8, 31, 21, 54, 39, 227));
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            _progressDisplay.ShowExifUpdateResult(true);
            
            var output = GetOutput();
            
            // Assert
            Assert.Equal(expected, output);
        }
        
        [Fact]
        public void ShowExifUpdateResult_バッファがクリアされること()
        {
            // Arrange
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowExifUpdateResult(true);
            
            // Act
            // 2回目は新しい出力として扱われるはず
            _progressDisplay.StartProcessing("test2");
            _progressDisplay.ShowExifUpdateResult(false);
            
            var output = GetOutput().Split(Environment.NewLine);
            
            // Assert
            Assert.Equal(3, output.Length); // 2行 + 空行
            Assert.Contains("test 撮影日時：更新済", output[0]);
            Assert.Contains("test2 撮影日時：スキップ", output[1]);
        }

        [Fact]
        public void ShowError_エラーメッセージを表示する()
        {
            // Arrange
            string errorMessage = "ファイルが存在しません";
            string expected = $"エラー: {errorMessage}" + Environment.NewLine;
            
            // Act
            _progressDisplay.ShowError(errorMessage);
            var output = GetErrorOutput();

            // Assert
            Assert.Equal(expected, output);
        }
        
        [Fact]
        public void ShowError_バッファにデータがある場合は先に出力する()
        {
            // Arrange
            string errorMessage = "ファイルが存在しません";
            
            // Act
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            _progressDisplay.ShowError(errorMessage);
            
            // Assert
            var output = GetOutput();
            var errorOutput = GetErrorOutput();
            
            Assert.Contains("test 作成日時：更新済 更新日時：更新済", output);
            Assert.Equal($"エラー: {errorMessage}", errorOutput.Trim());
        }
        
        [Fact]
        public void Dispose_バッファにデータがある場合は出力する()
        {
            // Arrange
            _progressDisplay.StartProcessing("test");
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            
            // Act
            _progressDisplay.Dispose();
            
            // Assert
            var output = GetOutput();
            Assert.Contains("test 作成日時：更新済", output);
            
            // 破棄後は何も出力されないこと
            _outputWriter.GetStringBuilder().Clear();
            _progressDisplay.ShowError("test");
            Assert.Equal(string.Empty, GetOutput());
        }
    }
}
