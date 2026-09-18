using System.Collections.Generic;

namespace mpv_winui.Modules.AppConfData;

public sealed class AppConfDataNode
{
    public AppConfDataNode(string name, string relativePath, bool isDirectory, bool exists = false)
    {
        Name = name;
        RelativePath = relativePath;
        IsDirectory = isDirectory;
        Exists = exists;
        Children = [];
    }

    public string Name
    {
        get;
    }

    public string RelativePath
    {
        get;
    }

    public bool IsDirectory
    {
        get;
    }

    public bool Exists
    {
        get; set;
    }

    public string Glyph => IsDirectory ? "\uE8B7" : "\uE7C3";

    public List<AppConfDataNode> Children
    {
        get;
    }
}