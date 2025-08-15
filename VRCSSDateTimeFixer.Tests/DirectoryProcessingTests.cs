using System;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer.Tests
{
    public class DirectoryProcessingTests
    {
        [Fact]
        public async Task ディレクトリ内のPNGとJPEGが対象としてカウントされること()
        {
            // Arrange: 一時ディレクトリとテストファイルを作成
            string tempDir = Path.Combine(Path.GetTempPath(), "VRCSSDateTimeFixer_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // VRChat 形式2 (.jpeg)
                File.WriteAllText(Path.Combine(tempDir, "VRChat_2025-08-11_01-38-55.893_1920x1080.jpeg"), "dummy");
                // VRChat 形式1 (.jpg)
                File.WriteAllText(Path.Combine(tempDir, "VRChat_1920x1080_2025-08-11_01-38-55.893.jpg"), "dummy");
                // VRChat 形式1 (.png)
                File.WriteAllText(Path.Combine(tempDir, "VRChat_1920x1080_2025-08-11_01-39-15.017.png"), "dummy");
                // 非対象
                File.WriteAllText(Path.Combine(tempDir, "not_target.txt"), "dummy");

                // 標準出力/標準エラー捕捉（型の初期化前に差し替え）
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                var originalOut = Console.Out;
                var originalErr = Console.Error;
                try
                {
                    Console.SetOut(writer);
                    Console.SetError(writer);

                    var root = Program.BuildCommandLine();

                        // Act
                    int exitCode = await root.InvokeAsync(new[] { tempDir });

                    writer.Flush();

                    // Assert: 3 ファイルの作成日時/最終更新日時がファイル名の日時に更新されている
                    Assert.Equal(0, exitCode);

                    string[] targets = new[]
                    {
                        Path.Combine(tempDir, "VRChat_2025-08-11_01-38-55.893_1920x1080.jpeg"),
                        Path.Combine(tempDir, "VRChat_1920x1080_2025-08-11_01-38-55.893.jpg"),
                        Path.Combine(tempDir, "VRChat_1920x1080_2025-08-11_01-39-15.017.png"),
                    };

                    foreach (var file in targets)
                    {
                        var expected = FileNameValidator.GetDateTimeFromFileName(Path.GetFileName(file));
                        var creation = File.GetCreationTime(file);
                        var lastWrite = File.GetLastWriteTime(file);

                        Assert.Equal(expected, creation);
                        Assert.Equal(expected, lastWrite);
                    }
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalErr);
                }
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }
}
