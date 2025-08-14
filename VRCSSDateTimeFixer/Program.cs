using System.CommandLine;
using VRCSSDateTimeFixer.Services;

namespace VRCSSDateTimeFixer
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("VRChatスクリーンショットのメタデータを更新するツール");

            // パス引数（必須）
            var pathArgument = new Argument<string>(
                name: "path",
                description: "処理するファイルまたはディレクトリのパス");

            // 再帰オプション
            var recursiveOption = new Option<bool>(
                name: "--recursive",
                description: "サブディレクトリも再帰的に処理します",
                getDefaultValue: () => false);
            
            // 再帰オプションのエイリアスを追加
            recursiveOption.AddAlias("-r");

            // コマンドに引数とオプションを追加
            rootCommand.AddArgument(pathArgument);
            rootCommand.AddOption(recursiveOption);

            // コマンドのハンドラを設定
            rootCommand.SetHandler(async (path, recursive) =>
            {
                try
                {
                    await ProcessPathAsync(path, recursive);
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"エラーが発生しました: {ex.Message}");
                    Environment.Exit(1);
                }
            }, pathArgument, recursiveOption);

            // コマンドを実行
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task ProcessPathAsync(string path, bool recursive)
        {
            if (File.Exists(path))
            {
                // ファイルを処理
                var result = await VRChatScreenshotProcessor.ProcessFileAsync(path);
                Console.WriteLine(result.Message);
                
                if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    await Console.Error.WriteLineAsync($"エラー: {result.ErrorMessage}");
                }
            }
            else if (Directory.Exists(path))
            {
                // ディレクトリ内のファイルを処理
                await ProcessDirectoryAsync(path, recursive);
            }
            else
            {
                throw new FileNotFoundException($"指定されたパスが見つかりません: {path}");
            }
        }

        private static async Task ProcessDirectoryAsync(string directoryPath, bool recursive)
        {
            var searchOption = recursive 
                ? SearchOption.AllDirectories 
                : SearchOption.TopDirectoryOnly;

            var files = Directory.EnumerateFiles(directoryPath, "*.png", searchOption);
            int processedCount = 0;
            int totalFiles = files.Count();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine($"処理を開始します: {directoryPath}");
            Console.WriteLine($"対象ファイル数: {totalFiles} 件");
            Console.WriteLine(new string('=', 80));

            foreach (var file in files)
            {
                try
                {
                    var result = await VRChatScreenshotProcessor.ProcessFileAsync(file);
                    Console.WriteLine(result.Message);
                    
                    if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        await Console.Error.WriteLineAsync($"エラー: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"エラーが発生しました ({file}): {ex.Message}");
                }
                
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
                
                await Task.Delay(10); // システム負荷軽減のための遅延
            }
            
            stopwatch.Stop();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"処理が完了しました: {processedCount} 件のファイルを処理しました");
            string totalTime = stopwatch.Elapsed.ToString("hh':'mm':'ss");
            Console.WriteLine($"所要時間: {totalTime}");
        }
    }
}
