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
    }
}
