using mpv_winui.Modules.Menu.MpvMenu;

namespace mpv_conf_test;

[TestFixture]
public class MenuConfParserTests
{
    private string _tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "mpv-menu-conf-" + Guid.NewGuid().ToString("N"));
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

    private string WriteTemp(string content)
    {
        var path = Path.Combine(_tempDir, "menu.conf");
        File.WriteAllText(path, content);
        return path;
    }

    // ---- file-level API ----

    [Test]
    public void Parse_NullPath_ReturnsNull()
    {
        Assert.That(MenuConfParser.Parse((string?)null), Is.Null);
    }

    [Test]
    public void Parse_EmptyPath_ReturnsNull()
    {
        Assert.That(MenuConfParser.Parse(string.Empty), Is.Null);
    }

    [Test]
    public void Parse_MissingFile_ReturnsNull()
    {
        Assert.That(MenuConfParser.Parse(Path.Combine(_tempDir, "missing.conf")), Is.Null);
    }

    [Test]
    public void Parse_EmptyFile_ReturnsEmptyList()
    {
        Assert.That(MenuConfParser.Parse(WriteTemp(string.Empty)), Is.Empty);
    }

    [Test]
    public void LinesOverload_MatchesFileRead()
    {
        var content = "Menu\n\tPlay\tplaylist-play\n";
        var fromFile = MenuConfParser.Parse(WriteTemp(content))!;
        var fromLines = MenuConfParser.Parse(content.Split('\n', StringSplitOptions.RemoveEmptyEntries));

        Assert.That(fromFile.Count, Is.EqualTo(fromLines.Count));
        Assert.That(fromFile[0].Name, Is.EqualTo(fromLines[0].Name));
        Assert.That(fromFile[0].Children![0].CommandString, Is.EqualTo(fromLines[0].Children![0].CommandString));
    }

    // ---- item parsing (mirrors mpv parse_menu_item / parse_menu_conf) ----

    [Test]
    public void ParsesSimpleItem()
    {
        var items = MenuConfParser.Parse(new[] { "Play\tplaylist-play" });

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Name, Is.EqualTo("Play"));
        Assert.That(items[0].CommandString, Is.EqualTo("playlist-play"));
        Assert.That(items[0].IsSeparator, Is.False);
        Assert.That(items[0].Children, Is.Null);
        Assert.That(items[0].Hidden, Is.Null);
    }

    [Test]
    public void ItemWithoutCommand_IsSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\tPlay\tplaylist-play" });

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Name, Is.EqualTo("Menu"));
        Assert.That(items[0].CommandString, Is.Null);
        Assert.That(items[0].Children, Has.Count.EqualTo(1));
        Assert.That(items[0].Children![0].Name, Is.EqualTo("Play"));
        Assert.That(items[0].Children![0].CommandString, Is.EqualTo("playlist-play"));
    }

    [Test]
    public void ParsesNestedSubmenus()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\tSub", "\t\tLeaf\tcmd" });

        var sub = items[0].Children![0];
        Assert.That(sub.Name, Is.EqualTo("Sub"));
        Assert.That(sub.Children, Has.Count.EqualTo(1));
        Assert.That(sub.Children![0].Name, Is.EqualTo("Leaf"));
        Assert.That(sub.Children![0].CommandString, Is.EqualTo("cmd"));
    }

    [Test]
    public void DeeperIndentJump_ParsesAsSingleLevel()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\t\tLeaf\tcmd" });

        Assert.That(items[0].Children, Has.Count.EqualTo(1));
        Assert.That(items[0].Children![0].Name, Is.EqualTo("Leaf"));
    }

    [Test]
    public void TwoSiblingSubmenus()
    {
        var items = MenuConfParser.Parse(new[] { "One", "\tA\tcmd", "Two", "\tB\tcmd" });

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items[0].Name, Is.EqualTo("One"));
        Assert.That(items[0].Children![0].Name, Is.EqualTo("A"));
        Assert.That(items[1].Name, Is.EqualTo("Two"));
        Assert.That(items[1].Children![0].Name, Is.EqualTo("B"));
    }

    [Test]
    public void BlankLine_IsSeparator()
    {
        var items = MenuConfParser.Parse(new[] { "A\tcmd", string.Empty, "B\tcmd" });

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0].Name, Is.EqualTo("A"));
        Assert.That(items[1].IsSeparator, Is.True);
        Assert.That(items[2].Name, Is.EqualTo("B"));
    }

    [Test]
    public void WhitespaceOnlyLine_IsSeparator()
    {
        var items = MenuConfParser.Parse(new[] { "   " });

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].IsSeparator, Is.True);
    }

    [Test]
    public void BlankLine_Trailing_IsDropped()
    {
        var items = MenuConfParser.Parse(new[] { "Play\tplay", string.Empty });

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Name, Is.EqualTo("Play"));
        Assert.That(items[0].IsSeparator, Is.False);
    }

    [Test]
    public void BlankLine_TakesDepthFromNextLine()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\tA\tcmd", string.Empty, "Two" });

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0].Name, Is.EqualTo("Menu"));
        Assert.That(items[0].Children, Has.Count.EqualTo(1));
        Assert.That(items[1].IsSeparator, Is.True);
        Assert.That(items[2].Name, Is.EqualTo("Two"));
    }

    [Test]
    public void BlankLine_NextLineDeeper_TakesDeeperDepth()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\tA\tcmd", string.Empty, "\tB\tcmd" });

        var children = items[0].Children!;
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(children, Has.Count.EqualTo(3));
        Assert.That(children[0].Name, Is.EqualTo("A"));
        Assert.That(children[1].IsSeparator, Is.True);
        Assert.That(children[2].Name, Is.EqualTo("B"));
    }

    [Test]
    public void HeaderCommentLines_AreTreatedAsItems()
    {
        var items = MenuConfParser.Parse(new[] { "# comment", "Play\tplay" });

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items[0].Name, Is.EqualTo("# comment"));
    }

    [Test]
    public void ParsesStateTokens()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\thidden=h\tdisabled=d\tchecked=c" });

        Assert.That(items[0].CommandString, Is.EqualTo("cmd"));
        Assert.That(items[0].Hidden, Is.EqualTo("h"));
        Assert.That(items[0].Disabled, Is.EqualTo("d"));
        Assert.That(items[0].Checked, Is.EqualTo("c"));
    }

    [Test]
    public void StateTokenWithoutCommand_LooksLikeSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Menu\thidden=cond" });

        Assert.That(items[0].CommandString, Is.Null);
        Assert.That(items[0].Hidden, Is.EqualTo("cond"));
        Assert.That(items[0].Children, Is.Empty);
    }

    [Test]
    public void WhitespaceBeforeStateToken_IsSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Item\t hidden=x" });

        Assert.That(items[0].CommandString, Is.Null);
        Assert.That(items[0].Hidden, Is.EqualTo("x"));
        Assert.That(items[0].Children, Is.Empty);
    }

    [Test]
    public void WhitespaceBeforeStateToken_AfterCommand()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\t checked=c" });

        Assert.That(items[0].CommandString, Is.EqualTo("cmd"));
        Assert.That(items[0].Checked, Is.EqualTo("c"));
    }

    [Test]
    public void StateValue_IsTrimmed()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\thidden= p && q  " });

        Assert.That(items[0].Hidden, Is.EqualTo("p && q"));
    }

    [Test]
    public void CommandContainingEquals_IsNotState()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tmode=insert" });

        Assert.That(items[0].CommandString, Is.EqualTo("mode=insert"));
        Assert.That(items[0].Hidden, Is.Null);
    }

    [Test]
    public void StateTokens_ReadInAnyOrder()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\tchecked=c\thidden=h" });

        Assert.That(items[0].Checked, Is.EqualTo("c"));
        Assert.That(items[0].Hidden, Is.EqualTo("h"));
    }

    [Test]
    public void LeadingWhitespace_CountsAsDepth()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "    Leaf\tcmd" });

        Assert.That(items[0].Children, Has.Count.EqualTo(1));
        Assert.That(items[0].Children![0].Name, Is.EqualTo("Leaf"));
    }

    [Test]
    public void ShallowerIndent_EndsSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Root", "\tA\tcmd", "B\tcmd" });

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items[0].Children, Has.Count.EqualTo(1));
        Assert.That(items[1].Name, Is.EqualTo("B"));
    }

    [Test]
    public void Separator_InsideSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Menu", "\tA\tcmd", string.Empty, "\tB\tcmd" });

        var children = items[0].Children!;
        Assert.That(children, Has.Count.EqualTo(3));
        Assert.That(children[0].Name, Is.EqualTo("A"));
        Assert.That(children[1].IsSeparator, Is.True);
        Assert.That(children[2].Name, Is.EqualTo("B"));
    }

    [Test]
    public void Separator_BetweenRootSubmenus()
    {
        var items = MenuConfParser.Parse(new[] { "One", "\tA\tcmd", string.Empty, "Two", "\tB\tcmd" });

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0].Name, Is.EqualTo("One"));
        Assert.That(items[1].IsSeparator, Is.True);
        Assert.That(items[2].Name, Is.EqualTo("Two"));
    }

    [Test]
    public void Title_IsTrimmed()
    {
        var items = MenuConfParser.Parse(new[] { "  Title  \tcmd" });

        Assert.That(items[0].Name, Is.EqualTo("Title"));
    }

    [Test]
    public void TrailingTab_IsSubmenu()
    {
        var items = MenuConfParser.Parse(new[] { "Item\t" });

        Assert.That(items[0].Name, Is.EqualTo("Item"));
        Assert.That(items[0].CommandString, Is.Null);
    }

    // ---- divergence from mpv, intentionally lenient ----

    [Test]
    public void UnknownTokenAfterCommand_IsIgnored()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\tgarbage" });

        Assert.That(items[0].CommandString, Is.EqualTo("cmd"));
        Assert.That(items[0].Hidden, Is.Null);
        Assert.That(items[0].Disabled, Is.Null);
        Assert.That(items[0].Checked, Is.Null);
    }

    [Test]
    public void UnknownStateToken_IsIgnored()
    {
        var items = MenuConfParser.Parse(new[] { "Item\tcmd\tcustom=x" });

        Assert.That(items[0].CommandString, Is.EqualTo("cmd"));
        Assert.That(items[0].Hidden, Is.Null);
        Assert.That(items[0].Disabled, Is.Null);
        Assert.That(items[0].Checked, Is.Null);
    }
}