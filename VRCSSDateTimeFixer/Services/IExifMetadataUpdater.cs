namespace VRCSSDateTimeFixer.Services
{
    public interface IExifMetadataUpdater
    {
        /// <summary>
        /// 画像ファイルのEXIFメタデータを更新します。
        /// </summary>
        /// <param name="filePath">画像ファイルのパス</param>
        /// <param name="dateTime">設定する日時</param>
        /// <returns>更新が成功した場合はtrue、それ以外はfalse</returns>
        Task<bool> UpdateExifMetadataAsync(string filePath, DateTime dateTime);
    }
}
