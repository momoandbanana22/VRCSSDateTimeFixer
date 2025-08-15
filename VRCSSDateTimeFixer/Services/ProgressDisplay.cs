using System.Text;

namespace VRCSSDateTimeFixer.Services
{
    /// <summary>
    /// ファイル処理の進捗をコンソールに表示するためのクラスです。
    /// このクラスはスレッドセーフではありません。
    /// </summary>
    public class ProgressDisplay : IDisposable
    {
        private readonly TextWriter _output;
        private readonly TextWriter _errorOutput;
        private readonly StringBuilder _outputBuffer = new();
        private bool _isDisposed;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public ProgressDisplay() : this(Console.Out, Console.Error)
        {
        }

        /// <summary>
        /// テスト用の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="output">標準出力の代わりに使用する<see cref="TextWriter"/></param>
        /// <param name="errorOutput">標準エラー出力の代わりに使用する<see cref="TextWriter"/></param>
        public ProgressDisplay(TextWriter output, TextWriter errorOutput)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _errorOutput = errorOutput ?? throw new ArgumentNullException(nameof(errorOutput));
        }

        /// <summary>
        /// ファイル処理の開始を表示します。
        /// </summary>
        /// <param name="fileName">処理中のファイル名</param>
        public void StartProcessing(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // バッファをクリアしてファイル名を追加
            _outputBuffer.Clear();
            _outputBuffer.Append(fileName);
        }

        /// <summary>
        /// ファイル名から抽出した日時を表示します。
        /// </summary>
        /// <param name="dateTime">抽出した日時</param>
        public void ShowExtractedDateTime(DateTime dateTime)
        {
            if (_isDisposed) return;
            // 日時をバッファに追加（コロン付き）
            _outputBuffer.Append($":{dateTime:yyyy年MM月dd日 HH時mm分ss.fff}");
        }

        /// <summary>
        /// 作成日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowCreationTimeUpdateResult(bool isUpdated)
        {
            if (_isDisposed) return;
            _outputBuffer.Append($" 作成日時：{(isUpdated ? "更新済" : "スキップ")}");
        }

        /// <summary>
        /// 更新日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowLastWriteTimeUpdateResult(bool isUpdated)
        {
            if (_isDisposed) return;
            _outputBuffer.Append($" 更新日時：{(isUpdated ? "更新済" : "スキップ")}");
        }

        /// <summary>
        /// Exif撮影日時の更新結果を表示します。
        /// </summary>
        /// <param name="isUpdated">更新が成功した場合はtrue、それ以外はfalse</param>
        public void ShowExifUpdateResult(bool isUpdated)
        {
            if (_isDisposed) return;
            _outputBuffer.Append($" 撮影日時：{(isUpdated ? "更新済" : "スキップ")}");
            try
            {
                _output.WriteLine(_outputBuffer.ToString());
            }
            catch (ObjectDisposedException)
            {
                // テスト環境等で出力が破棄されている場合は黙って無視
            }
            _outputBuffer.Clear();
        }

        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public void ShowError(string message)
        {
            if (_isDisposed) return;
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            // バッファに出力があれば先に出力
            if (_outputBuffer.Length > 0)
            {
                try
                {
                    _output.WriteLine(_outputBuffer.ToString());
                }
                catch (ObjectDisposedException)
                {
                    // テスト環境等で出力が破棄されている場合は黙って無視
                }
                _outputBuffer.Clear();
            }

            try
            {
                _errorOutput.WriteLine($"エラー: {message}");
            }
            catch (ObjectDisposedException)
            {
                // テスト環境等で出力が破棄されている場合は黙って無視
            }
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// <param name="disposing">マネージド リソースを解放する場合は true</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // マネージド リソースを解放
                    try
                    {
                        if (_outputBuffer.Length > 0)
                        {
                            _output.WriteLine(_outputBuffer.ToString());
                            _outputBuffer.Clear();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // 出力が既に破棄されている場合は無視
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// ファイナライザー
        /// </summary>
        ~ProgressDisplay()
        {
            Dispose(false);
        }
    }
}
