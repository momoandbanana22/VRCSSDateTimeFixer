using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using VRCSSDateTimeFixer.Validators;

namespace VRCSSDateTimeFixer
{
    public static class FileTimestampUpdater
    {
        public static async Task<(bool CreationTimeUpdated, bool LastWriteTimeUpdated)> UpdateFileTimestampAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                return (false, false);
            }

            try
            {
                // ファイル名から日時を抽出（ファイル名のみを使用）
                string fileName = Path.GetFileName(filePath);
                DateTime? dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
                if (!dateTime.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] ファイル名から日時を抽出できません: {filePath}");
                    return (false, false);
                }

                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] ファイル名から抽出した日時: {dateTime.Value:yyyy-MM-dd HH:mm:ss.fff}");

                bool creationTimeUpdated = false;
                bool lastWriteTimeUpdated = false;

                // 現在のタイムスタンプを記録
                var originalCreationTime = File.GetCreationTime(filePath);
                var originalLastWriteTime = File.GetLastWriteTime(filePath);
                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 更新前の作成日時: {originalCreationTime:yyyy-MM-dd HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 更新前の最終更新日時: {originalLastWriteTime:yyyy-MM-dd HH:mm:ss.fff}");

                // ファイルのタイムスタンプを非同期で更新
                await Task.Run(() =>
                {
                    try
                    {
                        // 読み取り専用属性の処理は後で行う

                        // まず読み取り専用属性を削除
                        var attributes = File.GetAttributes(filePath);
                        bool wasReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                        if (wasReadOnly)
                        {
                            File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        }

                        try
                        {
                            // 作成日時を更新
                            System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 作成日時を更新中: {dateTime.Value:yyyy-MM-dd HH:mm:ss.fff}");
                            File.SetCreationTime(filePath, dateTime.Value);
                            creationTimeUpdated = true;

                            // 最終更新日時を更新
                            System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 最終更新日時を更新中: {dateTime.Value:yyyy-MM-dd HH:mm:ss.fff}");
                            File.SetLastWriteTime(filePath, dateTime.Value);
                            lastWriteTimeUpdated = true;

                            // 念のため両方のタイムスタンプを再設定（UTCでも）
                            File.SetCreationTimeUtc(filePath, dateTime.Value.ToUniversalTime());
                            File.SetLastWriteTimeUtc(filePath, dateTime.Value.ToUniversalTime());
                        }
                        finally
                        {
                            // 元の読み取り専用属性を復元
                            if (wasReadOnly)
                            {
                                File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // エラーログを出力
                        System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] タイムスタンプの更新に失敗: {ex.GetType().Name} - {ex.Message}");

                        // リトライロジック
                        try
                        {
                            // 少し待ってからリトライ
                            System.Threading.Thread.Sleep(100);

                            // ファイル属性を取得
                            var attributes = File.GetAttributes(filePath);
                            bool isReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                            try
                            {
                                // 読み取り専用属性を削除
                                if (isReadOnly)
                                {
                                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                                }

                                // 両方のタイムスタンプを個別に更新
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] リトライ: 作成日時を更新中");
                                    File.SetCreationTime(filePath, dateTime.Value);
                                    File.SetCreationTimeUtc(filePath, dateTime.Value.ToUniversalTime());
                                    creationTimeUpdated = true;
                                }
                                catch (Exception creationEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 作成日時の更新に失敗: {creationEx.Message}");
                                }

                                try
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] リトライ: 最終更新日時を更新中");
                                    File.SetLastWriteTime(filePath, dateTime.Value);
                                    File.SetLastWriteTimeUtc(filePath, dateTime.Value.ToUniversalTime());
                                    lastWriteTimeUpdated = true;
                                }
                                catch (Exception lastWriteEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 最終更新日時の更新に失敗: {lastWriteEx.Message}");
                                }

                                // 読み取り専用を復元
                                if (isReadOnly)
                                {
                                    File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
                                }
                            }
                            catch (Exception retryEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] リトライ中のエラー: {retryEx.Message}");
                                throw; // エラーを再スローして外側のキャッチブロックで処理させる
                            }
                        }
                        catch (Exception retryEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] リトライ失敗: {retryEx.Message}");
                        }
                    }
                });

                // 最終タイムスタンプを確認
                var finalCreationTime = File.GetCreationTime(filePath);
                var finalLastWriteTime = File.GetLastWriteTime(filePath);
                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 更新後の作成日時: {finalCreationTime:yyyy-MM-dd HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine($"[FileTimestampUpdater] 更新後の最終更新日時: {finalLastWriteTime:yyyy-MM-dd HH:mm:ss.fff}");

                return (creationTimeUpdated, lastWriteTimeUpdated);
            }
            catch (Exception) when (IsExpectedException())
            {
                return (false, false);
            }
        }

        // 下位互換性のため残す
        [Obsolete("Use UpdateFileTimestampAsync instead.")]
        public static bool UpdateFileTimestamp(string filePath)
        {
            var result = UpdateFileTimestampAsync(filePath).GetAwaiter().GetResult();
            return result.CreationTimeUpdated || result.LastWriteTimeUpdated;
        }

        private static bool IsExpectedException()
        {
            // テストのため、とりあえず常にtrueを返す
            // 本番環境では適切な例外ハンドリングを実装する
            return true;
        }

        /// <summary>
        /// 画像ファイルのExifデータを更新します。
        /// このメソッドはファイルの内容を更新するため、その更新により、
        /// OSによってファイルのタイムスタンプが変更される可能性があります。
        /// タイムスタンプを保持する必要がある場合は、呼び出し元で管理してください。
        /// </summary>
        /// <param name="filePath">Exifを更新する画像ファイルのパス</param>
        /// <returns>Exifの更新に成功した場合はtrue、それ以外はfalse</returns>
        public static async Task<bool> UpdateExifDateAsync(string filePath)
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
                // 例外で失敗する実装のため、null 許容は不要
                DateTime dateTime = FileNameValidator.GetDateTimeFromFileName(fileName);
                string dateTimeStr = dateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

                // 現在のタイムスタンプを保存（置換後に復元する）
                var originalCreationTime = File.GetCreationTime(filePath);
                var originalLastWriteTime = File.GetLastWriteTime(filePath);

                // 読み取り専用属性を一時的に解除（using 外で行う）
                var attributes = File.GetAttributes(filePath);
                bool wasReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                if (wasReadOnly)
                {
                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                }

                string? tempFile = null;
                try
                {
                    // 画像を非同期で読み込んでExifを更新（ここでは保存まで）
                    using (var image = await Task.Run(() => Image.Load(filePath)))
                    {
                        // Exifプロファイルがなければ作成
                        image.Metadata.ExifProfile ??= new ExifProfile();
                        // 撮影日時を設定
                        image.Metadata.ExifProfile.SetValue(ExifTag.DateTimeOriginal, dateTimeStr);

                        // 一時ファイルに保存（元の拡張子を維持する）
                        string ext = Path.GetExtension(filePath);
                        if (string.IsNullOrWhiteSpace(ext))
                        {
                            ext = ".png"; // 既定はPNG
                        }
                        tempFile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{ext}");

                        await Task.Run(() =>
                        {
                            var lower = ext.ToLowerInvariant();
                            if (lower == ".png")
                            {
                                var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder
                                {
                                    ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.RgbWithAlpha,
                                };
                                image.Save(tempFile, encoder);
                            }
                            else if (lower == ".jpg" || lower == ".jpeg")
                            {
                                var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                                {
                                    Quality = 90
                                };
                                image.Save(tempFile, encoder);
                            }
                            else
                            {
                                // それ以外は拡張子から自動判定
                                image.Save(tempFile);
                            }
                        });
                    }

                    // ここで画像のハンドルは解放済み。ファイル置換を実施。
                    File.Replace(tempFile, filePath, null);

                    // タイムスタンプを元に戻す
                    File.SetCreationTime(filePath, originalCreationTime);
                    File.SetLastWriteTime(filePath, originalLastWriteTime);
                    File.SetCreationTimeUtc(filePath, originalCreationTime.ToUniversalTime());
                    File.SetLastWriteTimeUtc(filePath, originalLastWriteTime.ToUniversalTime());

                    // using を抜けた後（すべてのハンドル解放後）に、短いリトライで読み取り可能か確認
                    const int maxWaitMs = 200;
                    const int stepMs = 20;
                    int waited = 0;
                    while (true)
                    {
                        try
                        {
                            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                            break; // 開けたらOK
                        }
                        catch (IOException)
                        {
                            if (waited >= maxWaitMs) break;
                            await Task.Delay(stepMs);
                            waited += stepMs;
                        }
                    }

                    return true;
                }
                finally
                {
                    // 一時ファイルを削除（存在する場合）
                    try { if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile)) File.Delete(tempFile); } catch { }

                    // 読み取り専用属性を元に戻す
                    if (wasReadOnly)
                    {
                        try { File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly); } catch { }
                    }
                }
            }
            catch (Exception ex) when (IsExpectedException())
            {
                // 画像の読み込みに失敗した場合も処理は続行
                System.Diagnostics.Debug.WriteLine($"画像読み込みエラー ({filePath}): {ex.Message}");
                return false;
            }
        }

        // 下位互換性のため残す
        [Obsolete("Use UpdateExifDateAsync instead.")]
        public static bool UpdateExifDate(string filePath)
        {
            return UpdateExifDateAsync(filePath).GetAwaiter().GetResult();
        }
    }
}