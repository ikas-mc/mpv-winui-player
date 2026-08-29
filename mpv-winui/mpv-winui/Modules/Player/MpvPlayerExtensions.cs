using mpv_winrt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public enum RepeatState
    {
        All,
        One,
        None
    }

    public enum FileType
    {
        File,
        Folder,
        Url,
        Other,
    }

    public record FileItem(string Path, FileType Type = FileType.File);

    public enum OpenMode
    {
        Replace,
        Append,
        InsertNext,
    }

    public static class MpvPlayerExtensions
    {
        public static Task InitializeAsync(this MpvPlayer player, string configFolder, int volume, DisplayColorKind colorKind, int refreshRate)
        {
            return Task.Run(() => { player.Initialize(configFolder, 1, 1, volume, colorKind, refreshRate); });
        }

        public static Task InitializeForPreviewAsync(this MpvPlayer player)
        {
            return Task.Run(() => { player.InitializeForPreview(1, 1); });
        }

        public static RepeatState GetRepeatState(this MpvPlayer player)
        {
            if (player.LoopFile())
            {
                return RepeatState.One;
            }

            if (player.LoopPlaylist())
            {
                return RepeatState.All;
            }

            return RepeatState.None;
        }

        public static void SetRepeatState(this MpvPlayer player, RepeatState state)
        {
            player.LoopFile(state == RepeatState.One);
            player.SetLoopPlaylist(state == RepeatState.All);
        }

        public static void Open(this MpvPlayer player, FileItem file, OpenMode action = OpenMode.Replace)
        {
            player.Open((FileItem[])[file], action);
        }

        public static void Open(this MpvPlayer player, IReadOnlyList<FileItem> files, OpenMode action = OpenMode.Replace)
        {
            if (files?.Count > 0)
            {
                var allSub = true;
                foreach (var file in files)
                {
                    if (file.Type != FileType.File)
                    {
                        allSub = false;
                        break;
                    }
                }

                if (allSub)
                {
                    var extensions = player.GetSubtitleExtensions();
                    if (extensions?.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            if (!IsSubtitleFile(file.Path, extensions))
                            {
                                allSub = false;
                                break;
                            }
                        }

                        if (allSub)
                        {
                            foreach (var file in files)
                            {
                                player.Command(["osd-auto", "sub-add", file.Path]);
                            }
                            return;
                        }
                    }
                }

                switch (action)
                {
                    case OpenMode.Replace:
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            player.Command(["osd-auto", "loadfile", files[i].Path, i == 0 ? "replace+play" : "append"]);
                        }
                        break;
                    }

                    case OpenMode.Append:
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            player.Command(["osd-auto", "loadfile", files[i].Path, "append"]);
                        }
                        break;
                    }

                    case OpenMode.InsertNext:
                    {
                        for (int i = files.Count - 1; i >= 0; i--)
                        {
                            player.Command(["osd-auto", "loadfile", files[i].Path, i > 0 ? "insert-next" : "insert-next-play"]);
                        }
                        break;
                    }
                }
            }
        }

        public static Task OpenAsync(this MpvPlayer player, FileItem file, OpenMode action = OpenMode.Replace)
        {
            return Task.Run(() => player.Open(file, action));
        }

        public static Task OpenAsync(this MpvPlayer player, IReadOnlyList<FileItem> files, OpenMode action = OpenMode.Replace)
        {
            return Task.Run(() => player.Open(files, action));
        }

        public static async ValueTask RunCommandAsync(this MpvPlayer player, IList<string> args)
        {
            if (args?.Count > 0)
            {
                await Task.Run(() => player.Command(args));
            }
        }

        public static async ValueTask RunCommandAsync(this MpvPlayer player, string cmd)
        {
            if (!string.IsNullOrEmpty(cmd))
            {
                await Task.Run(() => player.CommandString(cmd));
            }
        }

        private static bool IsSubtitleFile(string path, IReadOnlyList<string> extensions)
        {
            if (string.IsNullOrEmpty(path) || extensions is null || extensions.Count == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(path)?.TrimStart('.');
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            foreach (var item in extensions)
            {
                if (string.Equals(item, ext, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}