using Xunit;

namespace VRCSSDateTimeFixer.Tests
{
    // コンソール出力をキャプチャするテストは並列実行を無効化して相互干渉を防ぐ
    [CollectionDefinition("ConsoleCapture", DisableParallelization = true)]
    public class ConsoleCaptureCollection
    {
    }
}
