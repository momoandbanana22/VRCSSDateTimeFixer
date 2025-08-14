namespace VRCSSDateTimeFixer.Services
{
    /// <summary>
    /// ファイル処理の進捗をコンソールに表示するためのクラスです。
    /// このクラスはスレッドセーフではありません。
    /// </summary>
    public class ProgressDisplay
    {
        /// <summary>
        /// ファイル処理の開始を表示します。
        /// </summary>
        /// <param name="fileName">処理中のファイル名</param>
        public void StartProcessing(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // ファイル名を出力（改行なし）
            Console.Write(fileName);
        }

        /// <summary>
        /// ファイル名から抽出した日時を表示します。
        /// </summary>
        /// <param name="dateTime">抽出した日時</param>
        public void ShowExtractedDateTime(DateTime dateTime)
        {
            // 日時を出力（コロン付き、改行なし）
            Console.Write($":{dateTime:yyyy年MM月dd日 HH時mm分ss.fff}");
        }

        /// <summary>
        /// 作成日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowCreationTimeUpdateResult(bool isUpdated)
        {
            // 作成日時更新前の表示（先頭にスペース、改行なし）
            Console.Write($" 作成日時：");
            // 作成日時更新後の表示（改行なし）
            Console.Write($"{(isUpdated ? "更新済" : "スキップ")}");
        }

        /// <summary>
        /// 更新日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowLastWriteTimeUpdateResult(bool isUpdated)
        {
            // 更新日時更新前の表示（先頭にスペース、改行なし）
            Console.Write($" 更新日時：");
            // 更新日時更新後の表示（改行なし）
            Console.Write($"{(isUpdated ? "更新済" : "スキップ")}");
        }

        /// <summary>
        /// Exif撮影日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowExifUpdateResult(bool isUpdated)
        {
            // 撮影日時更新前の表示（先頭にスペース、改行なし）
            Console.Write($" 撮影日時：");
            // 撮影日時更新後の表示（最後に改行）
            Console.WriteLine($"{(isUpdated ? "更新済" : "スキップ")}");
        }

        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            Console.Error.WriteLine($"エラー: {message}");
        }
    }
}
