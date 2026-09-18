using mpv_winui.Modules.AppConfData;
using mpv_winui.Modules.FileSystem;
using System.IO.Compression;

namespace mpv_conf_test;

[TestFixture]
public class AppConfDataImportServiceTests
{
    private string _root = null!;
    private string _zipPath = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "mpv-appconfdata-test-" + Guid.NewGuid().ToString("N"));
        AppData.Root = _root;
        _zipPath = Path.Combine(_root, "sample-conf-data.zip");
        Directory.CreateDirectory(_root);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private void CreateZip(Dictionary<string, string> files)
    {
        var source = Path.Combine(_root, "zip-src");
        Directory.CreateDirectory(source);
        foreach (var file in files)
        {
            var path = Path.Combine(source, file.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, file.Value);
        }

        using var archive = ZipFile.Open(_zipPath, ZipArchiveMode.Create);
        var dirs = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var dir = Path.GetDirectoryName(file.Key.Replace('\\', '/')) ?? string.Empty;
            while (!string.IsNullOrEmpty(dir))
            {
                dirs.Add(dir);
                dir = Path.GetDirectoryName(dir) ?? string.Empty;
            }
        }

        foreach (var dir in dirs)
        {
            archive.CreateEntry(dir.Replace('\\', '/') + "/");
        }

        foreach (var file in files)
        {
            archive.CreateEntryFromFile(Path.Combine(source, file.Key), file.Key.Replace('\\', '/'));
        }
    }

    [Test]
    public async Task ReadArchive_ReturnsTree()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv/mpv.conf"] = "profile=default",
            ["mpv/input.conf"] = "u = cycle sub",
            ["mpvw.conf"] = "[default]",
        });

        var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(_zipPath);

        Assert.That(roots.Count, Is.EqualTo(2));
        var mpv = roots.First(n => n.Name == "mpv");
        Assert.That(mpv.IsDirectory, Is.True);
        Assert.That(mpv.Children.Count, Is.EqualTo(2));
        Assert.That(mpv.Children.Select(c => c.Name), Is.EqualTo(["mpv.conf", "input.conf"]));
        Assert.That(roots.First(n => n.Name == "mpvw.conf").IsDirectory, Is.False);
    }

    [Test]
    public async Task ReadArchive_UsesBackslashRelativePaths()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv/mpv.conf"] = "profile=default",
        });

        var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(_zipPath);
        var mpv = roots.First(n => n.Name == "mpv");

        Assert.That(mpv.RelativePath, Is.EqualTo("mpv"));
        Assert.That(mpv.Children[0].RelativePath, Is.EqualTo(@"mpv\mpv.conf"));
    }

    [Test]
    public async Task Import_SelectedPathsOnly_WritesFiles()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv/mpv.conf"] = "profile=default",
            ["mpv/input.conf"] = "u = cycle sub",
            ["mpvw.conf"] = "[default]",
        });

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, ["mpv/mpv.conf", "mpvw.conf"], overwrite: true);

        Assert.That(count, Is.EqualTo(2));
        Assert.That(File.ReadAllText(Path.Combine(_root, "mpv", "mpv.conf")), Is.EqualTo("profile=default"));
        Assert.That(File.Exists(Path.Combine(_root, "mpv", "input.conf")), Is.False);
        Assert.That(File.Exists(Path.Combine(_root, "mpvw.conf")), Is.True);
    }

    [Test]
    public async Task Import_OverwriteTrue_ReplacesExisting()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv.conf"] = "profile=imported",
        });

        Directory.CreateDirectory(Path.Combine(_root, "mpv"));
        File.WriteAllText(Path.Combine(_root, "mpv.conf"), "profile=old");

        await AppConfDataImportService.Instance.ImportAsync(_zipPath, ["mpv.conf"], overwrite: true);

        Assert.That(File.ReadAllText(Path.Combine(_root, "mpv.conf")), Is.EqualTo("profile=imported"));
    }

    [Test]
    public async Task Import_OverwriteFalse_SkipsExisting()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv.conf"] = "profile=imported",
        });

        File.WriteAllText(Path.Combine(_root, "mpv.conf"), "profile=old");

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, ["mpv.conf"], overwrite: false);

        Assert.That(count, Is.EqualTo(0));
        Assert.That(File.ReadAllText(Path.Combine(_root, "mpv.conf")), Is.EqualTo("profile=old"));
    }

    [Test]
    public async Task Import_PathTraversal_SkipsEntry()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        using (var archive = ZipFile.Open(_zipPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("mpv.conf").WriteAll("profile=default");
            archive.CreateEntry("../escape.conf").WriteAll("evil");
        }

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, ["mpv.conf", "../escape.conf"], overwrite: true);

        Assert.That(count, Is.EqualTo(1));
        Assert.That(File.Exists(Path.Combine(_root, "mpv.conf")), Is.True);
    }

    [Test]
    public async Task Import_EmptySelection_ReturnsZero()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv.conf"] = "profile=default",
        });

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, new List<string>(), overwrite: true);

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task ReadArchive_EmptyOrNullPath_ReturnsEmpty()
    {
        Assert.That(await AppConfDataImportService.Instance.ReadArchiveAsync(null!), Is.Empty);
        Assert.That(await AppConfDataImportService.Instance.ReadArchiveAsync(string.Empty), Is.Empty);
    }

    [Test]
    public async Task ReadArchive_MissingZip_Throws()
    {
        Assert.ThrowsAsync<FileNotFoundException>(() => AppConfDataImportService.Instance.ReadArchiveAsync(Path.Combine(_root, "missing.zip")));
    }

    [Test]
    public async Task ReadArchive_SkipsPathTraversalEntry()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        using (var archive = ZipFile.Open(_zipPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("mpv/").WriteAll("");
            archive.CreateEntry("mpv/mpv.conf").WriteAll("profile=default");
            archive.CreateEntry("../escape.conf").WriteAll("evil");
        }

        var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(_zipPath);

        Assert.That(roots.Count, Is.EqualTo(1));
        Assert.That(roots[0].Name, Is.EqualTo("mpv"));
        Assert.That(roots[0].Children, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Import_NestedDirectory_CreatesParents()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv/scripts/a.lua"] = "a=1",
            ["mpv/scripts/b.lua"] = "b=2",
        });

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, [@"mpv\scripts\a.lua", @"mpv\scripts\b.lua"], overwrite: true);

        Assert.That(count, Is.EqualTo(2));
        Assert.That(File.ReadAllText(Path.Combine(_root, "mpv", "scripts", "a.lua")), Is.EqualTo("a=1"));
        Assert.That(File.ReadAllText(Path.Combine(_root, "mpv", "scripts", "b.lua")), Is.EqualTo("b=2"));
    }

    [Test]
    public async Task Import_SelectedPathNotInArchive_Skipped()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv.conf"] = "profile=default",
        });

        var count = await AppConfDataImportService.Instance.ImportAsync(_zipPath, ["mpv.conf", "missing.conf"], overwrite: true);

        Assert.That(count, Is.EqualTo(1));
        Assert.That(File.Exists(Path.Combine(_root, "missing.conf")), Is.False);
    }

    [Test]
    public async Task GetTargetFolder_ReturnsAppDataFolder()
    {
        var folder = await AppConfDataImportService.Instance.GetTargetFolderAsync();

        Assert.That(folder.Path, Is.EqualTo(_root));
    }

    [Test]
    public async Task ReadArchive_ExistingTargetFile_MarksExists()
    {
        CreateZip(new Dictionary<string, string>
        {
            ["mpv/mpv.conf"] = "profile=default",
            ["mpv/input.conf"] = "u = cycle sub",
        });

        Directory.CreateDirectory(Path.Combine(_root, "mpv"));
        File.WriteAllText(Path.Combine(_root, "mpv", "mpv.conf"), "profile=default");

        var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(_zipPath);
        var mpv = roots.First(n => n.Name == "mpv");
        Assert.That(mpv.Children.First(n => n.Name == "mpv.conf").Exists, Is.True);
        Assert.That(mpv.Children.First(n => n.Name == "input.conf").Exists, Is.False);
        Assert.That(mpv.Exists, Is.True);
    }

    [Test]
    public async Task ReadArchive_DirectoryEntriesWithSlash_PreservesEmptyFolders()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        using (var archive = ZipFile.Open(_zipPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("mpv/");
            archive.CreateEntry("mpv/empty/");
            archive.CreateEntry("mpv/mpv.conf").WriteAll("profile=default");
        }

        var roots = await AppConfDataImportService.Instance.ReadArchiveAsync(_zipPath);

        Assert.That(roots.Count, Is.EqualTo(1));
        var mpv = roots[0];
        Assert.That(mpv.IsDirectory, Is.True);
        Assert.That(mpv.Name, Is.EqualTo("mpv"));
        Assert.That(mpv.Children.Select(c => c.Name), Is.EqualTo(["empty", "mpv.conf"]));
        Assert.That(mpv.Children.First(c => c.Name == "empty").IsDirectory, Is.True);
        Assert.That(mpv.Children.First(c => c.Name == "empty").Children, Is.Empty);
    }
}

internal static class ZipArchiveEntryExtensions
{
    public static void WriteAll(this ZipArchiveEntry entry, string content)
    {
        using var writer = new StreamWriter(entry.Open());
        writer.Write(content);
    }
}