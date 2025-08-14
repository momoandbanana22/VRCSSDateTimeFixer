using Moq;
using System;
using System.IO;
using Xunit;
using VRCSSDateTimeFixer.Services;

namespace VRCSSDateTimeFixer.Tests.Services
{
    public class ProgressDisplayTests : IDisposable
    {
        private readonly StringWriter _outputWriter;
        private readonly TextWriter _originalOutput;
        private readonly TextWriter _originalError;
        private readonly ProgressDisplay _progressDisplay;

        public ProgressDisplayTests()
        {
            _outputWriter = new StringWriter();
            _originalOutput = Console.Out;
            _originalError = Console.Error;
            Console.SetOut(_outputWriter);
            Console.SetError(_outputWriter);
            _progressDisplay = new ProgressDisplay();
        }

        public void Dispose()
        {
            _outputWriter.Dispose();
            Console.SetOut(_originalOutput);
            Console.SetError(_originalError);
        }

        [Fact]
        public void StartProcessing_ファイル名を表示する()
        {
            // Arrange
            string fileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";

            // Act
            _progressDisplay.StartProcessing(fileName);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(fileName, output.Trim());
        }

        [Fact]
        public void ShowExtractedDateTime_日時を表示する()
        {
            // Arrange
            var dateTime = new DateTime(2022, 8, 31, 21, 54, 39, 227);
            string expected = ":2022年08月31日 21時54分39.227";

            // Act
            _progressDisplay.ShowExtractedDateTime(dateTime);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(expected, output.Trim());
        }

        [Fact]
        public void ShowCreationTimeUpdateResult_更新成功時に更新済みを表示する()
        {
            // Arrange
            string expected = "作成日時：更新済";

            // Act
            _progressDisplay.ShowCreationTimeUpdateResult(true);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(expected, output.Trim());
        }

        [Fact]
        public void ShowLastWriteTimeUpdateResult_更新成功時に更新済みを表示する()
        {
            // Arrange
            string expected = "更新日時：更新済";

            // Act
            _progressDisplay.ShowLastWriteTimeUpdateResult(true);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(expected, output.Trim());
        }

        [Fact]
        public void ShowExifUpdateResult_更新成功時に更新済みを表示する()
        {
            // Arrange
            string expected = " 撮影日時：更新済" + Environment.NewLine;

            // Act
            _progressDisplay.ShowExifUpdateResult(true);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(expected, output);
        }

        [Fact]
        public void ShowError_エラーメッセージを表示する()
        {
            // Arrange
            string errorMessage = "ファイルが存在しません";
            string expected = $"エラー: {errorMessage}" + Environment.NewLine;

            // Act
            _progressDisplay.ShowError(errorMessage);
            var output = _outputWriter.ToString();

            // Assert
            Assert.Equal(expected, output);
        }
    }
}
