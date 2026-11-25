using System.Text.Json.Serialization;

namespace mpv_winui.Modules.Language
{
    public partial class AppLang
    {
        [JsonIgnore]
        public string AppName { get; } = "mpvw";

        [JsonIgnore]
        public string AppVersion { get; } = "1.2";

        public string Ok { get; set; } = "OK";
        public string Cancel { get; set; } = "Cancel";
        public string Close { get; set; } = "Close";
        public string Confirm { get; set; } = "Confirm";
        public string Yes { get; set; } = "Yes";
        public string No { get; set; } = "No";

        public string Add { get; set; } = "Add";
        public string Apply { get; set; } = "Apply";
        public string Delete { get; set; } = "Delete";
        public string Clear { get; set; } = "Clear";
        public string Copy { get; set; } = "Copy";
        public string Remove { get; set; } = "Remove";
        public string Rename { get; set; } = "Rename";
        public string Restore { get; set; } = "Restore";
        public string Reset { get; set; } = "Reset";
        public string Edit { get; set; } = "Edit";
        public string Execute { get; set; } = "Execute";
        public string Save { get; set; } = "Save";

        public string Open { get; set; } = "Open";
        public string OpenFile { get; set; } = "Open File";
        public string OpenFolder { get; set; } = "Open Folder";
        public string OpenUrl { get; set; } = "Open URL";
        public string OpenDisc { get; set; } = "Open Disc";
        public string OpenFromClipboard { get; set; } = "Open from Clipboard";

        public string File { get; set; } = "File";
        public string View { get; set; } = "View";
        public string Play { get; set; } = "Play";
        public string Pause { get; set; } = "Pause";
        public string Stop { get; set; } = "Stop";
        public string Playlist { get; set; } = "Playlist";
        public string Quit { get; set; } = "Quit";
        public string Restart { get; set; } = "Restart";
        public string Refresh { get; set; } = "Refresh";
        public string Reload { get; set; } = "Reload";
        public string Search { get; set; } = "Search";

        public string Settings { get; set; } = "Settings";
        public string Options { get; set; } = "Options";
        public string Help { get; set; } = "Help";
        public string About { get; set; } = "About";
        public string Version { get; set; } = "Version";

        public string Import { get; set; } = "Import";
        public string Export { get; set; } = "Export";
        public string Download { get; set; } = "Download";
        public string ImportData { get; set; } = "Import Data";

        public string Name { get; set; } = "Name";
        public string NewName { get; set; } = "New name";
        public string Command { get; set; } = "Command";
        public string Value { get; set; } = "Value";
        public string Key { get; set; } = "Key";
        public string Profile { get; set; } = "Profile";
        public string Group { get; set; } = "Group";
        public string All { get; set; } = "All";
        public string Enabled { get; set; } = "Enabled";
        public string Modified { get; set; } = "Modified";

        public string Off { get; set; } = "Off";
        public string Auto { get; set; } = "Auto";

        public string Video { get; set; } = "Video";
        public string Audio { get; set; } = "Audio";
        public string Image { get; set; } = "Image";
        public string Archive { get; set; } = "Archive";
    }
}
