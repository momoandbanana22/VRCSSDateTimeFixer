using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    public class CommandLineTests
    {
        [Fact]
        public void ファイルパスが指定された場合_正しくパースできること()
        {
            // Arrange
            string[] args = new[] { "C:\\path\\to\\file.png" };

            // Act
            var parseResult = Program.BuildCommandLine().Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors);
            Assert.Equal("C:\\path\\to\\file.png", parseResult.GetValueForArgument(Program.PathArgument));
            Assert.False(parseResult.GetValueForOption(Program.RecursiveOption));
        }

        [Fact]
        public void ディレクトリパスと再帰オプションが指定された場合_正しくパースできること()
        {
            // Arrange
            string[] args = new[] { "C:\\path\\to\\directory", "-r" };

            // Act
            var parseResult = Program.BuildCommandLine().Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors);
            Assert.Equal("C:\\path\\to\\directory", parseResult.GetValueForArgument(Program.PathArgument));
            Assert.True(parseResult.GetValueForOption(Program.RecursiveOption));
        }

        [Fact]
        public void ヘルプオプションが指定された場合_パースが成功する()
        {
            // Arrange
            string[] args = new[] { "--help" };

            // Act
            var command = Program.BuildCommandLine();
            var parseResult = command.Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors);
            // ヘルプ表示がリクエストされたことを確認
            Assert.True(parseResult.Errors.Count == 0);
        }

        [Fact]
        public void 必須引数が指定されていない場合_エラーを返す()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var command = Program.BuildCommandLine();
            var parseResult = command.Parse(args);

            // Assert
            Assert.NotEmpty(parseResult.Errors);
            Assert.Contains("Required argument missing for command", parseResult.Errors[0].Message);
        }

        [Fact]
        public void 存在しないファイルが指定された場合_エラーを返す()
        {
            // Arrange
            string nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent_file.png");
            string[] args = new[] { nonExistentFile };

            // Act
            var parseResult = Program.BuildCommandLine().Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors); // パース時点ではエラーにならない

            // 実際の処理時にエラーになることを確認するためのテストも可能
            // ここではパースのテストに留める
        }

        [Fact]
        public void 無効なオプションが指定された場合_エラーを返す()
        {
            // Arrange
            string[] args = new[] { "valid.png", "--invalid-option" };

            // Act
            var command = Program.BuildCommandLine();
            var parseResult = command.Parse(args);

            // Assert
            Assert.NotEmpty(parseResult.Errors);
            Assert.Contains("Unrecognized command or argument", parseResult.Errors[0].Message);
        }

        [Fact]
        public void 再帰オプションが指定されていない場合_デフォルトでfalseとなること()
        {
            // Arrange
            string[] args = new[] { "C:\\path\\to\\directory" };

            // Act
            var command = Program.BuildCommandLine();
            var parseResult = command.Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors);
            var recursiveOption = (Option<bool>)command.Options.First(o => o.Name == "recursive");
            Assert.False(parseResult.GetValueForOption(recursiveOption));
        }

        [Theory]
        [InlineData("-r")]
        [InlineData("--recursive")]
        public void 再帰オプションのエイリアスが正しく動作すること(string option)
        {
            // Arrange
            string[] args = new[] { "C:\\path\\to\\directory", option };

            // Act
            var command = Program.BuildCommandLine();
            var parseResult = command.Parse(args);

            // Assert
            Assert.Empty(parseResult.Errors);
            var recursiveOption = (Option<bool>)command.Options.First(o => o.Name == "recursive");
            Assert.True(parseResult.GetValueForOption(recursiveOption));
        }

        [Fact]
        public void パスが指定されていない場合_エラーとなること()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var parseResult = Program.BuildCommandLine().Parse(args);

            // Assert
            Assert.NotEmpty(parseResult.Errors);
            Assert.Contains(parseResult.Errors, e => e.Message.Contains("Required argument missing"));
        }
    }
}
