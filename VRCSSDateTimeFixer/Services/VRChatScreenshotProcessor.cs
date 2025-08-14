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
            ".png",
            ".jpg",
            ".jpeg"
        };

        public static async Task<ProcessResult> ProcessFileAsync(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // バリデーション
            var validationResult = ValidateFile(filePath);
            if (!validationResult.IsValid)
            {
                return ProcessResult.Failure(fileName, validationResult.ErrorMessage);
            }

            // ファイル名から日時を抽出
            DateTime? dateTime;
            try
            {
                dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
                if (!dateTime.HasValue)
                {
                    return ProcessResult.Failure(fileName,
                        string.Format(ErrorMessages.InvalidFileNameFormat, fileName));
                }
            }
            catch (ArgumentException ex) when (ex.ParamName == "fileName")
            {
                return ProcessResult.Failure(fileName,
                    string.Format(ErrorMessages.InvalidFileNameFormat, fileName));
            }

            // 結果オブジェクトを作成
            var result = ProcessResult.CreateSuccess(fileName, dateTime.Value, false, false, false);

            // Exif情報を更新（サポートされている画像ファイルの場合）
            if (IsSupportedImageFile(filePath))
            {
                bool exifUpdated = await FileTimestampUpdater.UpdateExifDateAsync(filePath);
                result.SetExifUpdated(exifUpdated);
            }

            // ファイルのタイムスタンプを更新（Exif更新後に実行して、最終的なタイムスタンプを設定）
            var timestampResult = await FileTimestampUpdater.UpdateFileTimestampAsync(filePath);
            result.SetCreationTimeUpdated(timestampResult.CreationTimeUpdated);
            result.SetLastWriteTimeUpdated(timestampResult.LastWriteTimeUpdated);

            // 進捗を表示
            var progressDisplay = new ProgressDisplay();
            progressDisplay.StartProcessing(result.FileName);

            // CreateSuccessメソッドでExtractedDateTimeにnullを設定していないため、nullチェックは不要だが、念のため
            if (result.ExtractedDateTime is not DateTime extractedDateTime)
            {
                throw new InvalidOperationException("ExtractedDateTime should not be null for successful processing");
            }

            progressDisplay.ShowExtractedDateTime(extractedDateTime);
            progressDisplay.ShowCreationTimeUpdateResult(result.CreationTimeUpdated);
            progressDisplay.ShowLastWriteTimeUpdateResult(result.LastWriteTimeUpdated);
            progressDisplay.ShowExifUpdateResult(result.ExifUpdated);

            return result;
        }

        // 下位互換性のため残す
        [Obsolete("Use ProcessFileAsync instead.")]
        public static ProcessResult ProcessFile(string filePath)
        {
            return ProcessFileAsync(filePath).GetAwaiter().GetResult();
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

        // DisplayProgress メソッドは削除し、ProgressDisplay クラスを使用する
    }

    public class ProcessResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string FileName { get; }
        public DateTime? ExtractedDateTime { get; }
        public bool CreationTimeUpdated { get; private set; }
        public bool LastWriteTimeUpdated { get; private set; }
        public bool ExifUpdated { get; private set; }
        public string ErrorMessage { get; private set; }

        private ProcessResult(bool success, string fileName, string message, DateTime? extractedDateTime,
                           bool creationTimeUpdated, bool lastWriteTimeUpdated, bool exifUpdated, string? errorMessage = null)
        {
            Success = success;
            FileName = fileName;
            Message = message;
            ExtractedDateTime = extractedDateTime;
            CreationTimeUpdated = creationTimeUpdated;
            LastWriteTimeUpdated = lastWriteTimeUpdated;
            ExifUpdated = exifUpdated;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static ProcessResult CreateSuccess(string fileName, DateTime dateTime,
            bool creationTimeUpdated, bool lastWriteTimeUpdated, bool exifUpdated)
        {
            string dateTimeStr = dateTime.ToString("yyyy年MM月dd日 HH時mm分ss.fff");
            string message = $"{fileName}：{dateTimeStr} 作成日時：{(creationTimeUpdated ? "更新済" : "スキップ")} " +
                           $"更新日時：{(lastWriteTimeUpdated ? "更新済" : "スキップ")} " +
                           $"撮影日時：{(exifUpdated ? "更新済" : "スキップ")}";

            return new ProcessResult(true, fileName, message, dateTime,
                creationTimeUpdated, lastWriteTimeUpdated, exifUpdated);
        }

        public static ProcessResult Failure(string fileName, string errorMessage)
        {
            return new ProcessResult(false, fileName, $"{fileName}: {errorMessage}",
                null, false, false, false, errorMessage);
        }

        public void SetCreationTimeUpdated(bool updated) => CreationTimeUpdated = updated;
        public void SetLastWriteTimeUpdated(bool updated) => LastWriteTimeUpdated = updated;
        public void SetExifUpdated(bool updated) => ExifUpdated = updated;
    }
}
