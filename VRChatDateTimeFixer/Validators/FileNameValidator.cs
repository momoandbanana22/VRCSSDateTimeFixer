using System;
using System.Text.RegularExpressions;

namespace VRChatDateTimeFixer.Validators
{
    /// <summary>
    /// VRChatのスクリーンショットファイル名を検証するバリデータ
    /// </summary>
    public static class FileNameValidator
    {
        // フォーマット1: VRChat_幅x高さ_YYYY-MM-DD_HH-mm-ss.fff.png
        // 例: VRChat_1920x1080_2022-08-31_21-54-39.227.png
        private static readonly Regex Format1Regex = new(
            @"^VRChat_\d{1,5}x\d{1,5}_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\.\d{3}\.png$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// ファイル名がVRChatのスクリーンショットフォーマット1に準拠しているか検証します。
        /// </summary>
        /// <param name="fileName">検証するファイル名</param>
        /// <returns>フォーマットが有効な場合はtrue、それ以外はfalse</returns>
        public static bool IsValidFormat1(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            return IsValidFormat1Pattern(fileName);
        }

        private static bool IsValidFormat1Pattern(string fileName)
        {
            return Format1Regex.IsMatch(fileName);
        }
    }
}
