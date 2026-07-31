using mpv_winrt;

namespace mpv_winrt_test
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests
    {
        [Test]
        public void MpvInitialize()
        {
            var volume = 30;
            MpvPlayer mpvPlayer = new();
            mpvPlayer.Initialize("", 1, 1, volume, DisplayColorKind.SDR, 60);

            Assert.That(mpvPlayer.Volume(), Is.EqualTo(volume));
        }
    }
}
