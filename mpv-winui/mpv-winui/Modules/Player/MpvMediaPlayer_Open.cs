using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public enum FileType
    {
        File,
        Folder,
        Url,
    }

    public record FileItem(string Path, FileType Type = FileType.File);

    public enum OpenMode
    {
        Replace,
        Append,
        InsertNext,
    }

    public partial class MpvMediaPlayer
    {
        public bool IsSubtitleFile(string path)
        {
            var ext = Path.GetExtension(path);
            return !string.IsNullOrEmpty(ext) && _subtitleExtensions.Value.Contains(ext);
        }

        public void Open(FileItem file, OpenMode action = OpenMode.Replace)
        {
            Open((FileItem[])[file], action);
        }

        public void Open(IReadOnlyList<FileItem> files, OpenMode action = OpenMode.Replace)
        {
            if (files?.Count > 0)
            {
                bool allSub = true;
                foreach (var file in files)
                {
                    if (file.Type == FileType.Folder || !IsSubtitleFile(file.Path))
                    {
                        allSub = false;
                        break;
                    }
                }

                if (allSub)
                {
                    foreach (var file in files)
                    {
                        _mpvPlayer.Command(["osd-auto", "sub-add", file.Path]);
                    }
                    return;
                }

                switch (action)
                {
                    case OpenMode.Replace:
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            _mpvPlayer.Command(["osd-auto", "loadfile", files[i].Path, i == 0 ? "replace+play" : "append"]);
                        }
                        break;
                    }

                    case OpenMode.Append:
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            _mpvPlayer.Command(["osd-auto", "loadfile", "append"]);
                        }
                        break;
                    }

                    case OpenMode.InsertNext:
                    {
                        for (int i = files.Count - 1; i >= 0; i--)
                        {
                            _mpvPlayer.Command(["osd-auto", "loadfile", files[i].Path, i > 0 ? "insert-next" : "insert-next-play"]);
                        }
                        break;
                    }
                }
            }
        }

        public Task OpenAsync(FileItem file, OpenMode action = OpenMode.Replace)
        {
            return Task.Run(() => Open(file, action));
        }

        public Task OpenAsync(IReadOnlyList<FileItem> files, OpenMode action = OpenMode.Replace)
        {
            return Task.Run(() => Open(files, action));
        }
    }
}
