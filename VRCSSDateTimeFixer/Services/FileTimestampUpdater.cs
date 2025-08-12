// FileTimestampUpdater.cs

using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer
{
    public static class FileTimestampUpdater
    {
        public static bool UpdateFileTimestamp(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            string fileName = Path.GetFileName(filePath);
            try
            {
                // ファイル名から日時を抽出
                DateTime dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);

                // ファイルのタイムスタンプを更新
                File.SetCreationTime(filePath, dateTime);
                File.SetLastWriteTime(filePath, dateTime);

                return true;
            }
            catch (Exception) when (IsExpectedException())
            {
                return false;
            }
        }

        private static bool IsExpectedException()
        {
            // 権限不足や読み取り専用ファイルなどの予期される例外を判定
            return true;
        }
    }
}