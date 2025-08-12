// FileTimestampUpdaterTests.cs

using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    public class FileTimestampUpdaterTests
    {
        [Fact]
        public void 有効なファイル名から日時を抽出してタイムスタンプを更新できること()
        {
            // Given: テスト用の一時ファイルを作成
            string tempFile = Path.GetTempFileName();
            try
            {
                // テスト用のファイルを作成（既存のタイムスタンプを上書きするため）
                File.WriteAllText(tempFile, "test");

                // テスト対象の日時
                var expectedDate = new DateTime(2022, 8, 31, 21, 54, 39, 227);
                string tempDir = Path.GetDirectoryName(tempFile);
                string testFileName = Path.Combine(tempDir, "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                File.Move(tempFile, testFileName);

                // When: タイムスタンプを更新
                bool result = FileTimestampUpdater.UpdateFileTimestamp(testFileName);

                // Then: 更新が成功し、タイムスタンプが正しく設定されていること
                Assert.True(result);
                var fileInfo = new FileInfo(testFileName);
                Assert.Equal(expectedDate, fileInfo.CreationTime);
                Assert.Equal(expectedDate, fileInfo.LastWriteTime);
            }
            finally
            {
                // テスト用ファイルの後片付け
                if (File.Exists(tempFile)) File.Delete(tempFile);
                string testFile = Path.Combine(Path.GetTempPath(), "VRChat_1920x1080_2022-08-31_21-54-39.227.png");
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }
    }
}