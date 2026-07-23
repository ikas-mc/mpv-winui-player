namespace mpv_winui.Modules.Player
{
    public partial class MpvMediaPlayer
    {
        public void PlaylistPlayIndex(int index) => _mpvPlayer.Command(["playlist-play-index", index.ToString()]);

        public void PlaylistMove(int from, int to) => _mpvPlayer.Command(["playlist-move", from.ToString(), to.ToString()]);

        public void PlaylistRemove(int index) => _mpvPlayer.Command(["playlist-remove", index.ToString()]);

        public void PlaylistNext() => _mpvPlayer.Command(["playlist-next"]);

        public void PlaylistPrevious() => _mpvPlayer.Command(["playlist-prev"]);

        public void PlaylistShuffle() => _mpvPlayer.Command(["playlist-shuffle"]);
    }
}
