using VRCSSDateTimeFixer.Validators;
using Xunit;

namespace VRCSSDateTimeFixer.Tests.Validators
{
    public class FileNameValidatorTests
    {
        [Fact]
        public void フォーマット1_有効なファイル名_Trueを返すこと()
        {
            // Given: 有効なVRChatスクリーンショットファイル名
            string fileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat1(fileName);

            // Then: 有効なフォーマットと判定されること
            Assert.True(result);
        }

        [Fact]
        public void フォーマット1_無効なファイル名_Falseを返すこと()
        {
            // Given: 無効なファイル名
            string fileName = "Invalid_FileName_Format.txt";

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat1(fileName);

            // Then: 無効なフォーマットと判定されること
            Assert.False(result);
        }

        [Theory]
        [InlineData("VRChat_1x1_2022-01-01_00-00-00.000.png")] // 最小解像度
        [InlineData("VRChat_99999x99999_2022-12-31_23-59-59.999.png")] // 最大解像度
        [InlineData("VRChat_1920x1080_2020-02-29_00-00-00.000.png")] // うるう日
        public void フォーマット1_境界値_有効なパターン_Trueを返すこと(string fileName)
        {
            // Given: 境界値の有効なファイル名

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat1(fileName);

            // Then: 有効なフォーマットと判定されること
            Assert.True(result);
        }

        [Theory]
        [InlineData("VRChat_0x1080_2022-01-01_00-00-00.000.png")] // 幅が0
        [InlineData("VRChat_1920x0_2022-01-01_00-00-00.000.png")] // 高さが0
        [InlineData("VRChat_100000x1080_2022-01-01_00-00-00.000.png")] // 幅が大きすぎる
        [InlineData("VRChat_1920x100000_2022-01-01_00-00-00.000.png")] // 高さが大きすぎる
        [InlineData("VRChat_1920x1080_2021-02-29_00-00-00.000.png")] // 存在しない日付
        [InlineData("VRChat_1920x1080_2022-13-01_00-00-00.000.png")] // 無効な月
        [InlineData("VRChat_1920x1080_2022-01-32_00-00-00.000.png")] // 無効な日
        [InlineData("VRChat_1920x1080_2022-01-01_24-00-00.000.png")] // 無効な時間
        [InlineData("VRChat_1920x1080_2022-01-01_00-60-00.000.png")] // 無効な分
        [InlineData("VRChat_1920x1080_2022-01-01_00-00-60.000.png")] // 無効な秒
        public void フォーマット1_境界値_無効なパターン_Falseを返すこと(string fileName)
        {
            // Given: 無効な境界値のファイル名

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat1(fileName);

            // Then: 無効なフォーマットと判定されること
            Assert.False(result);
        }

        [Fact]
        public void フォーマット2_有効なファイル名_Trueを返すこと()
        {
            // Given: 有効なフォーマット2のファイル名
            string fileName = "VRChat_2022-08-31_21-54-39.227_1920x1080.png";

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat2(fileName);

            // Then: 有効なフォーマットと判定されること
            Assert.True(result);
        }

        [Theory]
        [InlineData("VRChat_2022-13-31_21-54-39.227_1920x1080.png")] // 無効な月
        [InlineData("VRChat_2022-02-30_21-54-39.227_1920x1080.png")] // 存在しない日付
        [InlineData("VRChat_2022-08-31_25-54-39.227_1920x1080.png")] // 無効な時間
        [InlineData("VRChat_2022-08-31_21-60-39.227_1920x1080.png")] // 無効な分
        [InlineData("VRChat_2022-08-31_21-54-60.227_1920x1080.png")] // 無効な秒
        [InlineData("VRChat_2022-08-31_21-54-39.227_0x1080.png")]    // 幅が0
        [InlineData("VRChat_2022-08-31_21-54-39.227_1920x0.png")]    // 高さが0
        [InlineData("VRChat_2022-08-31_21-54-39.227_100000x1080.png")] // 幅が大きすぎる
        [InlineData("VRChat_2022-08-31_21-54-39.227_1920x100000.png")] // 高さが大きすぎる
        [InlineData("Invalid_FileName_Format.png")]                   // 無効な形式
        public void フォーマット2_無効なファイル名_Falseを返すこと(string fileName)
        {
            // Given: 無効なファイル名

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat2(fileName);

            // Then: 無効なフォーマットと判定されること
            Assert.False(result);
        }

        [Theory]
        [InlineData("VRChat_2022-01-01_00-00-00.000_1x1.png")]          // 最小解像度
        [InlineData("VRChat_2022-12-31_23-59-59.999_99999x99999.png")]  // 最大解像度
        [InlineData("VRChat_2020-02-29_00-00-00.000_1920x1080.png")]    // うるう日
        [InlineData("vRcHaT_2022-08-31_21-54-39.227_1920X1080.PNG")]    // 大文字小文字混在
        public void フォーマット2_境界値_有効なパターン_Trueを返すこと(string fileName)
        {
            // Given: 境界値の有効なファイル名

            // When: フォーマット検証を実行
            bool result = FileNameValidator.IsValidFormat2(fileName);

            // Then: 有効なフォーマットと判定されること
            Assert.True(result);
        }

        #region ファイル名から日時を抽出する機能のテスト

        [Fact]
        public void 有効なフォーマット1のファイル名から日時を抽出できること()
        {
            // Given: 有効なフォーマット1のファイル名と期待される日時
            string fileName = "VRChat_1920x1080_2022-08-31_21-54-39.227.png";
            DateTime expected = new(2022, 8, 31, 21, 54, 39, 227);

            // When: 日時抽出を実行
            DateTime actual = FileNameValidator.GetDateTimeFromFileName(fileName);

            // Then: 期待通りの日時が抽出されること
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 有効なフォーマット2のファイル名から日時を抽出できること()
        {
            // Given: 有効なフォーマット2のファイル名と期待される日時
            string fileName = "VRChat_2022-08-31_21-54-39.227_1920x1080.png";
            DateTime expected = new(2022, 8, 31, 21, 54, 39, 227);

            // When: 日時抽出を実行
            DateTime actual = FileNameValidator.GetDateTimeFromFileName(fileName);

            // Then: 期待通りの日時が抽出されること
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Invalid_FileName_Format.png")]
        [InlineData("VRChat_1920x1080.png")]
        [InlineData("")]
        [InlineData(null)]
        public void 無効なファイル名の場合は例外をスローすること(string invalidFileName)
        {
            // Given: 無効なファイル名

            // When & Then: 例外がスローされることを確認
            Assert.Throws<ArgumentException>(() => FileNameValidator.GetDateTimeFromFileName(invalidFileName));
        }

        [Fact]
        public void うるう日を含むファイル名から正しく日時を抽出できること()
        {
            // Given: うるう日を含む有効なファイル名と期待される日時
            string fileName = "VRChat_1920x1080_2024-02-29_12-34-56.789.png";
            DateTime expected = new(2024, 2, 29, 12, 34, 56, 789);

            // When: 日時抽出を実行
            DateTime actual = FileNameValidator.GetDateTimeFromFileName(fileName);

            // Then: 期待通りの日時が抽出されること
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
