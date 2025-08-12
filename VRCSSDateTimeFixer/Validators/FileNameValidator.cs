using System.Globalization;
using System.Text.RegularExpressions;

// この名前空間は、VRChatのスクリーンショットファイル名の検証と解析を行う機能を提供します。
// 主に以下の機能を含みます：
// - ファイル名のフォーマット検証
// - 日付・時刻情報の抽出
// - 解像度情報の抽出

namespace VRCSSTimeFixer.Validators
{
    /// <summary>
    /// VRChatのスクリーンショットファイル名を検証するための静的クラスです。
    /// このクラスは、以下の形式のファイル名をサポートします：
    /// - 形式1: VRChat_幅x高さ_YYYY-MM-DD_HH-mm-ss.fff.png
    /// - 形式2: VRChat_YYYY-MM-DD_HH-mm-ss.fff_幅x高さ.png（将来の実装予定）
    /// </summary>
    /// <remarks>
    /// 使用例：
    /// <code>
    /// bool isValid = FileNameValidator.IsValid("VRChat_1920x1080_2022-08-31_21-54-39.227.png");
    /// </code>
    /// </remarks>
    public static class FileNameValidator
    {
        #region 定数

        /// <summary>
        /// ファイル名の正規表現パターン（形式1用）
        /// グループ名付きで以下の要素を抽出：
        /// - width: 画像の幅（ピクセル）
        /// - height: 画像の高さ（ピクセル）
        /// - year, month, day: 日付
        /// - hour, minute, second: 時刻
        /// 
        /// 形式: VRChat_幅x高さ_YYYY-MM-DD_HH-mm-ss.fff.png
        /// 例: VRChat_1920x1080_2022-08-31_21-54-39.227.png
        /// </summary>
        private const string RegexPattern =
            @"^VRChat_                  # 固定文字列
            (?<width>\d{1,5})          # 幅（1-5桁の数字）
            x                           # 区切り文字
            (?<height>\d{1,5})         # 高さ（1-5桁の数字）
            _                           # 区切り文字
            (?<year>\d{4})             # 年（4桁）
            -                           # 区切り文字
            (?<month>\d{2})            # 月（2桁）
            -                           # 区切り文字
            (?<day>\d{2})              # 日（2桁）
            _                           # 区切り文字
            (?<hour>\d{2})             # 時（2桁）
            -                           # 区切り文字
            (?<minute>\d{2})           # 分（2桁）
            -                           # 区切り文字
            (?<second>\d{2})           # 秒（2桁）
            \.\d{3}                    # ミリ秒（3桁）
            \.png$                      # 拡張子";

        /// <summary>
        /// 解像度の最小値（1ピクセル）
        /// </summary>
        private const int MinResolution = 1;

        /// <summary>
        /// 解像度の最大値（99,999ピクセル）
        /// 一般的なディスプレイ解像度を考慮して設定
        /// </summary>
        private const int MaxResolution = 99999;

        /// <summary>
        /// 形式1のファイル名を検証するための正規表現オブジェクト
        /// 
        /// オプション：
        /// - IgnorePatternWhitespace: パターン内の空白を無視（コメント用）
        /// - IgnoreCase: 大文字小文字を区別しない
        /// - Compiled: 正規表現をコンパイルして高速化（アプリケーション起動は少し遅くなるが、実行は高速）
        /// </summary>
        private static readonly Regex Format1Regex = new(
            RegexPattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// 指定されたファイル名が有効なVRChatスクリーンショットのファイル名かどうかを検証します。
        /// 
        /// このメソッドは以下の検証を行います：
        /// 1. ファイル名がnullまたは空でないこと
        /// 2. ファイル名が正規表現パターンに一致すること
        /// 3. 解像度が有効な範囲内であること
        /// 4. 日付・時刻が有効な値であること
        /// 
        /// 注意：
        /// - ファイルの存在確認は行いません
        /// - ファイルパスではなくファイル名のみを処理します
        /// </summary>
        /// <param name="fileName">検証するファイル名（フルパスまたはファイル名のみ）</param>
        /// <returns>
        /// ファイル名が有効なVRChatスクリーンショットの形式に一致し、
        /// かつ解像度・日時が有効な場合はtrue、
        /// それ以外の場合はfalse
        /// </returns>
        /// <example>
        /// <code>
        /// // 有効な形式の例
        /// bool isValid1 = FileNameValidator.IsValid("VRChat_1920x1080_2022-08-31_21-54-39.227.png");
        /// 
        /// // 無効な形式の例
        /// bool isValid2 = FileNameValidator.IsValid("invalid_file.png");
        /// </code>
        /// </example>
        public static bool IsValidFormat1(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var match = Format1Regex.Match(fileName);
            if (!match.Success)
            {
                return false;
            }

            return IsValidResolution(match) && IsValidDateTime(match);
        }

        #region プライベートメソッド

        /// <summary>
        /// 正規表現のマッチ結果から解像度を検証します。
        /// 
        /// このメソッドは、正規表現で抽出した幅と高さの値が
        /// 有効な範囲内（<see cref="MinResolution"/>～<see cref="MaxResolution"/>）
        /// であることを確認します。
        /// </summary>
        /// <param name="match">正規表現のマッチ結果</param>
        /// <returns>解像度が有効な範囲内の場合はtrue、それ以外はfalse</returns>
        private static bool IsValidResolution(Match match)
        {
            if (!TryGetResolution(match, out int width, out int height))
            {
                return false;
            }

            return IsValidResolutionValue(width) && IsValidResolutionValue(height);
        }

        /// <summary>
        /// 正規表現のマッチ結果から解像度を取得します。
        /// 
        /// このメソッドは、正規表現の名前付きグループから
        /// 幅と高さの数値をパースして返します。
        /// 
        /// 注意：
        /// パースに失敗した場合、outパラメータには0が設定されます。
        /// </summary>
        /// <param name="match">正規表現のマッチ結果</param>
        /// <param name="width">解析された幅（出力パラメータ）</param>
        /// <param name="height">解析された高さ（出力パラメータ）</param>
        /// <returns>両方の値が正常にパースできた場合はtrue、それ以外はfalse</returns>
        private static bool TryGetResolution(Match match, out int width, out int height)
        {
            width = 0;
            height = 0;

            return int.TryParse(match.Groups["width"].Value, out width) &&
                   int.TryParse(match.Groups["height"].Value, out height);
        }

        /// <summary>
        /// 解像度の値が有効な範囲内かどうかを検証します。
        /// 
        /// 有効な範囲は以下の通りです：
        /// - 最小値: <see cref="MinResolution"/> (1)
        /// - 最大値: <see cref="MaxResolution"/> (99999)
        /// 
        /// このメソッドは、解像度の値がこの範囲内にあるかどうかを
        /// パターンマッチングを使用して効率的にチェックします。
        /// </summary>
        /// <param name="value">検証する解像度の値</param>
        /// <returns>値が有効な範囲内の場合はtrue、それ以外はfalse</returns>
        private static bool IsValidResolutionValue(int value) =>
            value is >= MinResolution and <= MaxResolution;

        /// <summary>
        /// 正規表現のマッチ結果から日付と時刻を検証します。
        /// 
        /// このメソッドは以下の処理を行います：
        /// 1. 正規表現のマッチ結果から日付・時刻の各要素を抽出
        /// 2. 抽出した値をDateTime型としてパース可能か検証
        /// 3. 有効な日付・時刻かどうかを確認
        /// 
        /// 注意：
        /// - うるう年や月の日数は.NETのDateTimeクラスが自動的に検証します
        /// - タイムゾーンは考慮せず、ローカルタイムとして扱います
        /// </summary>
        /// <param name="match">正規表現のマッチ結果</param>
        /// <returns>有効な日付・時刻の場合はtrue、それ以外はfalse</returns>
        private static bool IsValidDateTime(Match match)
        {
            // 正規表現のマッチ結果から日付・時刻の各要素を取得
            // 例: "2022-08-31 21:54:39" の形式に変換
            string dateTimeString = $"{match.Groups["year"]}-{match.Groups["month"]}-{match.Groups["day"]} " +
                                 $"{match.Groups["hour"]}:{match.Groups["minute"]}:{match.Groups["second"]}";

            // サポートする日付時刻のフォーマットを指定
            // 現在は1つのフォーマットのみをサポート
            string[] formats = { "yyyy-MM-dd HH:mm:ss" };

            // DateTime.TryParseExactを使用して日付時刻の検証
            // 戻り値は不要なため、out _ で破棄
            return DateTime.TryParseExact(
                dateTimeString,          // 検証する日付時刻文字列
                formats,                 // 許可するフォーマットの配列
                CultureInfo.InvariantCulture,  // カルチャを指定（インバリアントカルチャを使用）
                DateTimeStyles.None,     // 追加の書式オプション（デフォルト）
                out _);                 // パース結果は不要なため破棄
        }

        #endregion
    }
}
