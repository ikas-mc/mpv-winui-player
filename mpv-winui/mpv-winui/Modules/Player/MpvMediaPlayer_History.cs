namespace mpv_winui.Modules.Player
{
    public partial class MpvMediaPlayer
    {
        public string? WatchHistoryPath
        {
            get
            {
                return _mpvPlayer.GetWatchHistoryPath();
            }
        }

        public string? WatchLaterFolderPath
        {
            get
            {
                return _mpvPlayer.GetWatchLaterFolderPath();
            }
        }

        public bool SaveWatchHistory => _mpvPlayer.SaveWatchHistory();
    }
}
