using System.CommandLine;
using VRCSSDateTimeFixer.Services;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer
{
    public class Program
    {
        // コマンドライン引数とオプションを定義
        public static readonly Argument<string> PathArgument = new(
            name: "path",
            description: "処理するファイルまたはディレクトリのパス");

        public static readonly Option<bool> RecursiveOption = new(
            aliases: new[] { "-r", "--recursive" },
            description: "サブディレクトリを再帰的に処理する",
            getDefaultValue: () => false);

        private static readonly ProgressDisplay _progressDisplay = new();

        public static int Main(string[] args)
        {
            var rootCommand = BuildCommandLine();
            return rootCommand.Invoke(args);
        }

        public static RootCommand BuildCommandLine()
        {
            var rootCommand = new RootCommand("VRChatのスクリーンショットのファイル名から日時情報を抽出し、ファイルのタイムスタンプとExif情報を更新します。");

            rootCommand.AddArgument(PathArgument);
            rootCommand.AddOption(RecursiveOption);

            rootCommand.SetHandler(async (path, recursive) =>
            {
                try
                {
                    await ProcessPathAsync(path, recursive);
                }
                catch (Exception ex)
                {
                    _progressDisplay.ShowError($"エラーが発生しました: {ex.Message}");
                    Environment.Exit(1);
                }
            }, PathArgument, RecursiveOption);

            return rootCommand;
        }

        private static async Task ProcessPathAsync(string path, bool recursive)
        {
            if (File.Exists(path))
            {
                await ProcessFileAsync(path);
            }
            else if (Directory.Exists(path))
            {
                await ProcessDirectoryAsync(path, recursive);
            }
            else
            {
                throw new FileNotFoundException($"指定されたパスが見つかりません: {path}");
            }
        }

        private static async Task ProcessFileAsync(string filePath)
        {
            try
            {
                _progressDisplay.StartProcessing(filePath);

                // ファイル名から日時を取得
                string fileName = Path.GetFileName(filePath);
                DateTime? dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);

                if (!dateTime.HasValue)
                {
                    _progressDisplay.ShowError($"{filePath}: ファイル名から日時を抽出できません");
                    return;
                }

                _progressDisplay.ShowExtractedDateTime(dateTime.Value);

                // ファイルのタイムスタンプを更新
                var (creationTimeUpdated, lastWriteTimeUpdated) = await FileTimestampUpdater.UpdateFileTimestampAsync(filePath);
                _progressDisplay.ShowCreationTimeUpdateResult(creationTimeUpdated);
                _progressDisplay.ShowLastWriteTimeUpdateResult(lastWriteTimeUpdated);

                // Exif情報を更新
                bool exifUpdated = await FileTimestampUpdater.UpdateExifDateAsync(filePath);
                _progressDisplay.ShowExifUpdateResult(exifUpdated);
            }
            catch (Exception ex)
            {
                _progressDisplay.ShowError($"{filePath}: {ex.Message}");
            }
        }

        private static async Task ProcessDirectoryAsync(string directoryPath, bool recursive)
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(directoryPath, "*.png", searchOption);

            int processedCount = 0;
            int totalFiles = files.Count();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _progressDisplay.StartProcessing(directoryPath);
            Console.WriteLine($"対象ファイル数: {totalFiles} 件");
            Console.WriteLine(new string('=', 80));

            foreach (var file in files)
            {
                await ProcessFileAsync(file);
                processedCount++;

                // 進捗表示（10ファイルごと、または最後のファイル）
                if (processedCount % 10 == 0 || processedCount == totalFiles)
                {
                    var elapsed = stopwatch.Elapsed;
                    var remaining = totalFiles > 0
                        ? TimeSpan.FromTicks(elapsed.Ticks * (totalFiles - processedCount) / processedCount)
                        : TimeSpan.Zero;

                    string elapsedTime = elapsed.ToString("hh':'mm':'ss");
                    string remainingTime = remaining.ToString("hh':'mm':'ss");
                    Console.WriteLine($"進捗: {processedCount}/{totalFiles} 件 ({processedCount * 100 / Math.Max(1, totalFiles)}%) " +
                                    $"経過: {elapsedTime} 残り: {remainingTime}");
                }
            }

            stopwatch.Stop();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"処理が完了しました: {processedCount} 件のファイルを処理しました");
            string totalTime = stopwatch.Elapsed.ToString("hh':'mm':'ss");
            Console.WriteLine($"所要時間: {totalTime}");
        }
    }
}
