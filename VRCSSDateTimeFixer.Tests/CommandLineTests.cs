using System.CommandLine;
using System.CommandLine.Parsing;
using VRCSSDateTimeFixer;
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
        public void ヘルプオプションが指定された場合_ヘルプを表示する()
        {
            // Arrange
            string[] args = new[] { "--help" };
            
            // Act
            var parseResult = Program.BuildCommandLine().Parse(args);
            
            // Assert
            Assert.Empty(parseResult.Errors);
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
