using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
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

        public static bool UpdateExifDate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            // ファイル名から日時を抽出
            string fileName = Path.GetFileName(filePath);
            try
            {
                DateTime dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
                string dateTimeStr = dateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

                // 画像を読み込んでExifを更新
                using (var image = Image.Load(filePath))
                {
                    // Exifプロファイルがなければ作成
                    image.Metadata.ExifProfile ??= new ExifProfile();

                    // 撮影日時を設定
                    image.Metadata.ExifProfile.SetValue(
                        ExifTag.DateTimeOriginal,
                        dateTimeStr);

                    // 変更を保存
                    image.Save(filePath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}