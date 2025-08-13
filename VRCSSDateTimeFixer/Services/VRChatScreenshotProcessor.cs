using System;
using System.Collections.Generic;
using System.IO;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer.Services
{
    // エラーメッセージを定数化
    internal static class ErrorMessages
    {
        public const string FilePathNotSpecified = "ファイルパスが指定されていません。";
        public const string FileNotFound = "ファイルが見つかりません: {0}";
        public const string FileIsReadOnly = "{0}: 読み取り専用ファイルのためスキップします";
        public const string UnsupportedFileFormat = "{0}: サポートされていないファイル形式です";
        public const string InvalidFileNameFormat = "{0}: ファイル名から日時を抽出できません";
        public const string SuccessMessage = "{0} を処理しました";
    }
    public static class VRChatScreenshotProcessor
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png"
        };

        public static ProcessResult ProcessFile(string filePath)
        {
            // バリデーション
            var validationResult = ValidateFile(filePath);
            if (!validationResult.IsValid)
            {
                return ProcessResult.Failure(validationResult.ErrorMessage);
            }

            string fileName = Path.GetFileName(filePath);
            DateTime? dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
            
            if (!dateTime.HasValue)
            {
                return ProcessResult.Failure(
                    string.Format(ErrorMessages.InvalidFileNameFormat, fileName));
            }

            // ファイルのタイムスタンプを更新
            bool timestampUpdated = FileTimestampUpdater.UpdateFileTimestamp(filePath);
            
            // Exif情報を更新
            bool exifUpdated = false;
            if (IsSupportedImageFile(filePath))
            {
                exifUpdated = FileTimestampUpdater.UpdateExifDate(filePath);
            }

            return ProcessResult.CreateSuccess(
                dateTime.Value,
                string.Format(ErrorMessages.SuccessMessage, fileName),
                timestampUpdated,
                exifUpdated);
        }

        private static (bool IsValid, string ErrorMessage) ValidateFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return (false, ErrorMessages.FilePathNotSpecified);
            }

            if (!File.Exists(filePath))
            {
                return (false, string.Format(ErrorMessages.FileNotFound, filePath));
            }

            if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                return (false, string.Format(ErrorMessages.FileIsReadOnly, Path.GetFileName(filePath)));
            }

            if (!IsSupportedImageFile(filePath))
            {
                return (false, string.Format(ErrorMessages.UnsupportedFileFormat, Path.GetFileName(filePath)));
            }

            return (true, string.Empty);
        }

        private static bool IsSupportedImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return SupportedExtensions.Contains(extension);
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
