using mpv_winui.Modules.Menu.MpvMenu;

namespace mpv_conf_test;

[TestFixture]
public class MenuConfWriterTests
{
    private string _tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "mpv-menu-writer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private async Task<string> WriteTemp(IEnumerable<MpvMenuItem> items)
    {
        var path = Path.Combine(_tempDir, "menu.conf");
        await MenuConfWriter.SaveAsync(path, items);
        return path;
    }

    private static string NormalizeNewLines(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    [Test]
    public async Task Save_WritesItemsWithTabIndentation()
    {
        var items = new List<MpvMenuItem>
        {
            new()
            {
                Name = "Menu",
                Children = new List<MpvMenuItem>
                {
                    new() { Name = "Play", CommandString = "playlist-play" },
                    new() { Name = "Fullscreen", CommandString = "cycle fullscreen" },
                },
            },
        };

        var path = await WriteTemp(items);
        var text = NormalizeNewLines(File.ReadAllText(path));

        Assert.That(text, Is.EqualTo("Menu\n\tPlay\tplaylist-play\n\tFullscreen\tcycle fullscreen\n"));
    }

    [Test]
    public async Task Save_WritesStatesInFixedOrder()
    {
        var items = new List<MpvMenuItem>
        {
            new() { Name = "Item", CommandString = "cmd", Hidden = "h", Disabled = "d", Checked = "c" },
        };

        var path = await WriteTemp(items);

        Assert.That(NormalizeNewLines(File.ReadAllText(path)), Is.EqualTo("Item\tcmd\thidden=h\tdisabled=d\tchecked=c\n"));
    }

    [Test]
    public async Task Save_WritesSeparatorAsBlankLine()
    {
        var items = new List<MpvMenuItem>
        {
            new() { Name = "Play", CommandString = "playlist-play" },
            new() { IsSeparator = true },
            new() { Name = "Stop", CommandString = "stop" },
        };

        var path = await WriteTemp(items);

        Assert.That(NormalizeNewLines(File.ReadAllText(path)), Is.EqualTo("Play\tplaylist-play\n\nStop\tstop\n"));
    }

    [Test]
    public async Task Save_EmptyStateValues_AreOmitted()
    {
        var items = new List<MpvMenuItem>
        {
            new() { Name = "Item", CommandString = "cmd", Hidden = string.Empty, Disabled = null, Checked = string.Empty },
        };

        var path = await WriteTemp(items);

        Assert.That(NormalizeNewLines(File.ReadAllText(path)), Is.EqualTo("Item\tcmd\n"));
    }

    [Test]
    public async Task Save_DeepNesting_IndentsCorrectly()
    {
        var items = new List<MpvMenuItem>
        {
            new()
            {
                Name = "Menu",
                Children = new List<MpvMenuItem>
                {
                    new()
                    {
                        Name = "Sub",
                        Children = new List<MpvMenuItem>
                        {
                            new() { Name = "Leaf", CommandString = "cmd" },
                        },
                    },
                },
            },
        };

        var path = await WriteTemp(items);

        Assert.That(NormalizeNewLines(File.ReadAllText(path)), Is.EqualTo("Menu\n\tSub\n\t\tLeaf\tcmd\n"));
    }

    [Test]
    public async Task Save_EmptyList_WritesEmptyFile()
    {
        var path = await WriteTemp(new List<MpvMenuItem>());

        Assert.That(File.ReadAllText(path), Is.Empty);
    }

    [Test]
    public async Task Save_CreatesMissingDirectories()
    {
        var path = Path.Combine(_tempDir, "nested", "deep", "menu.conf");

        await MenuConfWriter.SaveAsync(path, new List<MpvMenuItem> { new() { Name = "Item", CommandString = "cmd" } });

        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public async Task Save_OverwritesExistingFile()
    {
        var path = Path.Combine(_tempDir, "menu.conf");
        File.WriteAllText(path, "Stale\n");

        await MenuConfWriter.SaveAsync(path, new List<MpvMenuItem> { new() { Name = "Play", CommandString = "play" } });

        Assert.That(NormalizeNewLines(File.ReadAllText(path)), Is.EqualTo("Play\tplay\n"));
    }

    [Test]
    public async Task Roundtrip_ComplexStructure_PreservesTree()
    {
        var expected = new List<MpvMenuItem>
        {
            new()
            {
                Name = "Menu",
                Children = new List<MpvMenuItem>
                {
                    new() { Name = "Play", CommandString = "playlist-play" },
                    new() { IsSeparator = true },
                    new()
                    {
                        Name = "View",
                        Children = new List<MpvMenuItem>
                        {
                            new() { Name = "Fullscreen", CommandString = "cycle fullscreen", Hidden = "not-valid || !fullscreen", Checked = "fullscreen" },
                        },
                    },
                },
            },
            new() { Name = "Quit", CommandString = "quit", Disabled = "false" },
        };

        var path = await WriteTemp(expected);
        var actual = MenuConfParser.Parse(path)!;

        Assert.That(actual, Has.Count.EqualTo(expected.Count));
        for (var index = 0; index < expected.Count; index++)
        {
            AssertItemsEqual(expected[index], actual[index]);
        }
    }

    [Test]
    public async Task Roundtrip_EmptySubmenu_StaysSubmenu()
    {
        var expected = new List<MpvMenuItem>
        {
            new() { Name = "Menu", Children = new List<MpvMenuItem>() },
        };

        var path = await WriteTemp(expected);
        var actual = MenuConfParser.Parse(path)!;

        AssertItemsEqual(expected[0], actual[0]);
    }

    private static void AssertItemsEqual(MpvMenuItem expected, MpvMenuItem actual)
    {
        Assert.That(actual.Name, Is.EqualTo(expected.Name), "Name differs");
        Assert.That(actual.CommandString, Is.EqualTo(expected.CommandString), "Command differs");
        Assert.That(actual.IsSeparator, Is.EqualTo(expected.IsSeparator), "IsSeparator differs");
        Assert.That(actual.Hidden, Is.EqualTo(expected.Hidden), "Hidden differs");
        Assert.That(actual.Disabled, Is.EqualTo(expected.Disabled), "Disabled differs");
        Assert.That(actual.Checked, Is.EqualTo(expected.Checked), "Checked differs");

        if (expected.Children is null)
        {
            Assert.That(actual.Children, Is.Null, "Children should be null");
        }
        else
        {
            Assert.That(actual.Children, Is.Not.Null, "Children should not be null");
            Assert.That(actual.Children!.Count, Is.EqualTo(expected.Children.Count), "Children count differs");
            for (var index = 0; index < expected.Children.Count; index++)
            {
                AssertItemsEqual(expected.Children[index], actual.Children[index]);
            }
        }
    }
}