using System;
using System.IO;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer.Services
{
    public static class VRChatScreenshotProcessor
    {
        public static ProcessResult ProcessFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ProcessResult.Failure("ファイルパスが指定されていません。");
            }

            if (!File.Exists(filePath))
            {
                return ProcessResult.Failure($"ファイルが見つかりません: {filePath}");
            }

            // ファイル名から日時を抽出
            string fileName = Path.GetFileName(filePath);
            DateTime? dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
            
            if (!dateTime.HasValue)
            {
                return ProcessResult.Failure($"ファイル名から日時を抽出できません: {fileName}");
            }

            // ファイルのタイムスタンプを更新
            bool timestampUpdated = FileTimestampUpdater.UpdateFileTimestamp(filePath);
            
            // 画像ファイルの場合はExif情報も更新
            bool exifUpdated = false;
            if (Path.GetExtension(filePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                exifUpdated = FileTimestampUpdater.UpdateExifDate(filePath);
            }

            return ProcessResult.CreateSuccess(
                dateTime.Value,
                $"ファイルを処理しました: {fileName}",
                timestampUpdated,
                exifUpdated);
        }
    }

    public class ProcessResult
    {
        public bool Success { get; }
        public string Message { get; }
        public DateTime? ExtractedDateTime { get; }
        public bool TimestampUpdated { get; }
        public bool ExifUpdated { get; }

        private ProcessResult(bool success, string message, DateTime? extractedDateTime, bool timestampUpdated, bool exifUpdated)
        {
            Success = success;
            Message = message;
            ExtractedDateTime = extractedDateTime;
            TimestampUpdated = timestampUpdated;
            ExifUpdated = exifUpdated;
        }

        public static ProcessResult CreateSuccess(DateTime dateTime, string message, bool timestampUpdated, bool exifUpdated)
        {
            return new ProcessResult(true, message, dateTime, timestampUpdated, exifUpdated);
        }

        public static ProcessResult Failure(string message)
        {
            return new ProcessResult(false, message, null, false, false);
        }
    }
}
