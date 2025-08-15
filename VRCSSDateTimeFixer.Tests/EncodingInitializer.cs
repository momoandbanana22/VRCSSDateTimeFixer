using System.Runtime.CompilerServices;
using System.Text;

namespace VRCSSDateTimeFixer.Tests;

public static class EncodingInitializer
{
    // テストホスト起動時に UTF-8 を強制して日本語の文字化けを防止
    [ModuleInitializer]
    public static void Init()
    {
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.InputEncoding = Encoding.UTF8;
    }
}
