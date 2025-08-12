using VRChatDateTimeFixer.Validators;
using Xunit;

namespace VRChatDateTimeFixer.Tests.Validators
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
    }
}
