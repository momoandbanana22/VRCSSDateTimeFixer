using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace VRCSSDateTimeFixer.Services
{
    public class ExifMetadataUpdater : IExifMetadataUpdater
    {
        /// <summary>
        /// 画像ファイルのEXIFメタデータを更新します。
        /// </summary>
        /// <param name="filePath">画像ファイルのパス</param>
        /// <param name="dateTime">設定する日時</param>
        /// <returns>更新が成功した場合はtrue、それ以外はfalse</returns>
        public async Task<bool> UpdateExifMetadataAsync(string filePath, DateTime dateTime)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using (var image = await Image.LoadAsync(filePath))
                {
                    // Ensure the image has metadata
                    image.Metadata.ExifProfile ??= new ExifProfile();

                    // Set the DateTimeOriginal tag
                    var dateTimeString = dateTime.ToString("yyyy:MM:dd HH:mm:ss");
                    image.Metadata.ExifProfile.SetValue(
                        ExifTag.DateTimeOriginal,
                        dateTimeString);

                    // Save the image with updated metadata
                    await using var output = File.Create(filePath);
                    var format = image.Metadata.DecodedImageFormat ??
                        throw new InvalidOperationException("画像フォーマットを特定できませんでした");

                    await image.SaveAsync(output, format);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
