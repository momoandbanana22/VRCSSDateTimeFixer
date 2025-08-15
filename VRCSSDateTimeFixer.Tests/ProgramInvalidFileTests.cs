using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VRCSSDateTimeFixer;
using Xunit;

using Xunit.Abstractions;

namespace VRCSSDateTimeFixer.Tests
{
    [Collection("ConsoleCapture")]
    public class ProgramInvalidFileTests
    {
        [Fact]
        public async Task 無効なファイル名はスキップされ_1行のみ出力されること()
        {
            // Arrange
            string dir = Path.Combine(Path.GetTempPath(), "VRCSS_InvalidNameTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string invalid = Path.Combine(dir, "not_vrchat_format.jpg");
            File.WriteAllText(invalid, "dummy");

            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();
            var writerOut = new StringWriter(sbOut);
            var writerErr = new StringWriter(sbErr);
            var originalOut = Console.Out;
            var originalErr = Console.Error;

            try
            {
                Console.SetOut(writerOut);
                Console.SetError(writerErr);

                // Act
                await Program.ProcessFileAsync(invalid);

                writerOut.Flush();
                writerErr.Flush();

                // Assert
                var allOutLines = sbOut.ToString()
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                var fileName = Path.GetFileName(invalid);
                var outLines = allOutLines.Where(l => l.Contains(fileName, StringComparison.OrdinalIgnoreCase)).ToArray();
                var errLines = sbErr.ToString()
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                // 無効ファイルはエラーにはしない（スキップの1行のみ）
                Assert.True(errLines.Length == 0, "標準エラーには出力しない想定");
                Assert.True(outLines.Length == 1, $"対象ファイルの行は1行のみであるべきですが: {string.Join(" | ", outLines)}");
                Assert.Contains("スキップ", outLines[0]);
                Assert.DoesNotContain(fileName + fileName, outLines[0]);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
                try { File.Delete(invalid); } catch { }
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }
}
