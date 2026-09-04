using mpv_winui.Modules.MpvConf.Conf;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfManagerTests
{
    private string _dir = null!;

    [SetUp]
    public async Task SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-conf-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    private string WriteSample(string text)
    {
        string path = Path.Combine(_dir, "mpv.conf");
        File.WriteAllText(path, text);
        return path;
    }

    [Test]
    public async Task LoadAndSave_RoundTripsUnchangedFile()
    {
        string original =
            "### Logs\n" +
            "msg-level=all=error\n" +
            "log-file=~~/mpv.log\n" +
            "# osc=no\n" +
            "[mpvw-hdr]\n" +
            "profile-cond=p[\"user-data/mpv/color-kind\"] == \"HDR\"\n" +
            "\n" +
            "d3d11-output-format=rgb10_a2\n";

        string path = WriteSample(original);
        var manager = new MpvConfManager(path);
        manager.Load();
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test]
    public async Task ModifyValue_SavesOnlyThatLineChanged()
    {
        string path = WriteSample("msg-level=all=error\nlog-file=~~/mpv.log\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var option = manager.Get("msg-level")!;
        option.Value = "all=info";

        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("msg-level=all=info\nlog-file=~~/mpv.log\n"));
    }

    [Test]
    public async Task ToggleEnabled_AddsAndRemovesHash()
    {
        string path = WriteSample("osc=no\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var option = manager.Get("osc")!;
        option.Enabled = false;
        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("# osc=no\n"));

        manager.Load();
        var disabled = manager.Get("osc")!;
        Assert.That(disabled.Enabled, Is.False);
        disabled.Enabled = true;
        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("osc=no\n"));
    }

    [Test]
    public async Task InsertSection_AddsHeader_OnlyOnce()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertSection("newsec");
        manager.InsertSection("newsec");
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n[newsec]\n"));
        Assert.That(manager.Sections, Does.Contain("newsec"));
    }

    [Test]
    public async Task InsertOption_AppendsAtEndAndReindexes()
    {
        string path = WriteSample("a=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var added = manager.InsertOption("c", "3");
        Assert.That(added.LineNumber, Is.EqualTo(2));

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nb=2\nc=3\n"));

        var lines = manager.Lines;
        Assert.That(lines[0].LineNumber, Is.EqualTo(0));
        Assert.That(lines[2].LineNumber, Is.EqualTo(2));
    }

    [Test]
    public async Task InsertOption_InSection_AppendsAfterSectionLines()
    {
        string path = WriteSample("[sect]\na=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var added = manager.InsertOption("c", "3", "sect");
        Assert.That(added.Section, Is.EqualTo("sect"));

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("[sect]\na=1\nb=2\nc=3\n"));
    }

    [Test]
    public async Task InsertOption_IntoMissingSection_CreatesHeader()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("c", "3", "newsection");
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n[newsection]\nc=3\n"));
    }

    [Test]
    public async Task UnknownAndBlankLinesArePreservedOnSave()
    {
        string path = WriteSample("# note\n\nweird line here\nreal=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("# note\n\nweird line here\nreal=1\n"));
        Assert.That(manager.Lines[2].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public async Task InsertDisabled_WritesCommentPrefix()
    {
        string path = WriteSample(string.Empty);
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertDisabled("gpu-api", "d3d11");
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("# gpu-api=d3d11\n"));
    }

    [Test]
    public async Task GetAll_ReturnsRepeatedKeys()
    {
        string path = WriteSample("key=a\nkey=b\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.GetAll("key"), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Load_ReadsLineBased_WithAndWithoutTrailingNewline()
    {
        string withNewline = WriteSample("a=1\nb=2\n");
        var first = new MpvConfManager(withNewline);
        first.Load();

        string withoutNewline = Path.Combine(_dir, "plain.conf");
        File.WriteAllText(withoutNewline, "a=1\nb=2");
        var second = new MpvConfManager(withoutNewline);
        second.Load();

        Assert.That(first.Lines, Has.Count.EqualTo(2));
        Assert.That(second.Lines, Has.Count.EqualTo(2));
        Assert.That(first.Lines[1].Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(second.Lines[1].Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(first.Lines[1].Raw, Is.EqualTo(second.Lines[1].Raw));
    }

    [Test]
    public async Task Load_EmptyFile_ProducesNoLines()
    {
        string path = WriteSample(string.Empty);
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Lines, Is.Empty);
        Assert.That(manager.IsLoaded, Is.True);
    }

    [Test]
    public async Task Load_MissingFile_ProducesNoLinesAndIsLoaded()
    {
        var manager = new MpvConfManager(Path.Combine(_dir, "missing.conf"));
        manager.Load();

        Assert.That(manager.Lines, Is.Empty);
        Assert.That(manager.IsLoaded, Is.True);
    }

    [Test]
    public async Task Load_CrLfFile_ReadsOptionsAndValueWithoutCr()
    {
        string path = WriteSample("a=1\r\nb=2\r\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Get("a")!.Value, Is.EqualTo("1"));
        Assert.That(manager.Get("b")!.Value, Is.EqualTo("2"));
        Assert.That(manager.Lines, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Load_Utf8Bom_IsStrippedByReader()
    {
        string path = Path.Combine(_dir, "bom.conf");
        File.WriteAllText(path, "\uFEFFhwdec=auto\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Get("hwdec")!.Value, Is.EqualTo("auto"));
    }

    [Test]
    public async Task Save_NormalizesToTrailingNewline()
    {
        string path = Path.Combine(_dir, "plain.conf");
        File.WriteAllText(path, "a=1");
        var manager = new MpvConfManager(path);
        manager.Load();
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n"));
    }

    [Test]
    public async Task Save_WritesLfLineEndings()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n"));
        Assert.That(File.ReadAllText(path), Does.Not.Contain("\r"));
    }

    [Test]
    public async Task LoadAndSave_ReloadKeepsSameOptions()
    {
        string path = WriteSample("# note\n# osc=no\n[sec]\nprofile-cond=x\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        await manager.SaveAsync();

        var reloaded = new MpvConfManager(path);
        reloaded.Load();

        Assert.That(reloaded.Lines, Has.Count.EqualTo(manager.Lines.Count));
        Assert.That(reloaded.Get("osc")!.Enabled, Is.False);
        Assert.That(reloaded.Get("profile-cond", "sec")!.Value, Is.EqualTo("x"));
    }

    [Test]
    public async Task Remove_DeletesLineAndReindexes()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        Assert.That(manager.Remove(b), Is.True);
        Assert.That(manager.Get("b"), Is.Null);
        Assert.That(manager.Get("c")!.LineNumber, Is.EqualTo(1));

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nc=3\n"));
    }

    [Test]
    public async Task Remove_ReturnsFalseForForeignLine()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var other = new MpvConfManager(WriteSample("x=9\n"));
        other.Load();
        Assert.That(manager.Remove(other.Lines[0]), Is.False);
    }

    [Test]
    public async Task InsertOption_IntoDefaultProfile_GoesBeforeFirstSection()
    {
        string path = WriteSample("fs=yes\n[mpvw-sdr]\nprofile-cond=x\n[other]\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("hwdec", "auto", "");

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("fs=yes\nhwdec=auto\n[mpvw-sdr]\nprofile-cond=x\n[other]\n"));
    }

    [Test]
    public async Task InsertOption_IntoDefaultProfile_NoSections_AppendsAtEnd()
    {
        string path = WriteSample("fs=yes\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("hwdec", "auto", "");

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("fs=yes\nhwdec=auto\n"));
    }

    [Test]
    public async Task Remove_ExistingLine_BecomesDeleted_NotVisible()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        Assert.That(manager.Remove(b), Is.True);

        Assert.That(manager.Get("b"), Is.Null);
        Assert.That(manager.Options, Does.Not.Contain(b));
        Assert.That(manager.Lines, Does.Not.Contain(b));
        Assert.That(b.Status, Is.EqualTo(MpvConfLineStatus.Deleted));
        Assert.That(manager.DeletedLines, Does.Contain(b));
        Assert.That(manager.Get("c")!.LineNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task Remove_AddedLine_DropsEntirely()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var added = manager.InsertOption("b", "2");
        Assert.That(added.Status, Is.EqualTo(MpvConfLineStatus.Added));

        Assert.That(manager.Remove(added), Is.True);
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("b"), Is.Null);
        Assert.That(manager.Options.ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Restore_DeletedLine_ReturnsToExisting_KeepsPosition()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        manager.Remove(b);

        Assert.That(manager.Restore(b), Is.True);
        Assert.That(b.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(b.Modified, Is.False);
        Assert.That(manager.Get("b"), Is.SameAs(b));
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("b")!.LineNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task Restore_NonDeletedLine_ReturnsFalse()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Restore(manager.Get("a")!), Is.False);
    }

    [Test]
    public async Task Remove_AlreadyDeleted_ReturnsFalse()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var a = manager.Get("a")!;
        manager.Remove(a);
        Assert.That(manager.Remove(a), Is.False);
    }

    [Test]
    public async Task Save_RemovesDeletedLines_AndResetsStatus()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        manager.Remove(b);
        var added = manager.InsertOption("d", "4");

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nc=3\nd=4\n"));
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Lines, Does.Not.Contain(b));
        Assert.That(manager.Get("c")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("c")!.Modified, Is.False);
        Assert.That(manager.Get("d")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("d")!.Modified, Is.False);
    }

    [Test]
    public async Task InsertOption_AfterDeletion_RestoresInPlace()
    {
        string path = WriteSample("a=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        manager.Remove(b);

        var restored = manager.InsertOption("b", "9");
        Assert.That(restored, Is.SameAs(b));
        Assert.That(restored.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(restored.Value, Is.EqualTo("9"));
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("b")!.LineNumber, Is.EqualTo(1));

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nb=9\n"));
    }

    [Test]
    public async Task RemoveSection_FlagsHeaderOnly_KeepsMemberLineStates()
    {
        string path = WriteSample("a=1\n[sect]\nb=2\n[other]\nd=4\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.RemoveSection("sect"), Is.True);

        var b = manager.Get("b", "sect")!;
        Assert.That(b, Is.Not.Null);
        Assert.That(b.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(b.Modified, Is.False);
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Sections, Does.Contain("sect"));
        Assert.That(manager.IsSectionDeleted("sect"), Is.True);
        Assert.That(manager.IsSectionDeleted("other"), Is.False);
        Assert.That(manager.Get("a")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("d", "other")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));

        Assert.That(manager.RemoveSection("sect"), Is.False);
    }

    [Test]
    public async Task RemoveSection_PreservesAddedAndModifiedStates_UntilSave()
    {
        string path = WriteSample("[sect]\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.Get("b", "sect")!.Value = "9";
        manager.Get("b", "sect")!.Modified = true;
        var added = manager.InsertOption("d", "4", "sect");

        manager.RemoveSection("sect");

        Assert.That(manager.Get("b", "sect")!.Modified, Is.True);
        Assert.That(added.Status, Is.EqualTo(MpvConfLineStatus.Added));
        Assert.That(manager.Get("d", "sect"), Is.Not.Null);
        Assert.That(manager.DeletedLines, Is.Empty);
    }

    [Test]
    public async Task RemoveSection_SaveDropsSectionFromFileAndMemory()
    {
        string path = WriteSample("a=1\n[sect]\nb=2\n[other]\nd=4\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.RemoveSection("sect");
        await manager.SaveAsync();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n[other]\nd=4\n"));
        Assert.That(manager.Sections, Does.Not.Contain("sect"));
        Assert.That(manager.Get("b", "sect"), Is.Null);
        Assert.That(manager.Lines, Has.Count.EqualTo(3));
        Assert.That(manager.DeletedLines, Is.Empty);
    }

    [Test]
    public async Task RestoreSection_ClearsFlag_FileUnchangedAfterSave()
    {
        string original = "a=1\n[sect]\nb=2\n";
        string path = WriteSample(original);
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.RemoveSection("sect");
        Assert.That(manager.RestoreSection("sect"), Is.True);
        Assert.That(manager.IsSectionDeleted("sect"), Is.False);

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test]
    public async Task RestoreSection_DoesNotTouchIndividuallyDeletedLines()
    {
        string path = WriteSample("[sect]\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var c = manager.Get("c", "sect")!;
        manager.Remove(c);
        manager.RemoveSection("sect");

        manager.RestoreSection("sect");

        Assert.That(c.Status, Is.EqualTo(MpvConfLineStatus.Deleted));
        Assert.That(manager.Get("b", "sect"), Is.Not.Null);
        Assert.That(manager.Get("c", "sect"), Is.Null);

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("[sect]\nb=2\n"));
    }

    [Test]
    public async Task IsSectionDeleted_UnknownOrMissing_ReturnsFalse()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.IsSectionDeleted(""), Is.False);
        Assert.That(manager.IsSectionDeleted("nope"), Is.False);
    }

    [Test]
    public async Task RenameSection_RenamesHeaderRawAndOptionSections_PreservesState()
    {
        string path = WriteSample("[sect]\na=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.Get("a", "sect")!.Value = "9";
        manager.Get("a", "sect")!.Modified = true;
        manager.Remove(manager.Get("b", "sect")!);

        Assert.That(manager.RenameSection("sect", "renamed"), Is.True);

        var header = manager.Lines.First(l => l.Type == MpvConfLineType.Section && l.Section == "renamed");
        Assert.That(header.Raw, Is.EqualTo("[renamed]"));
        Assert.That(manager.Get("a", "renamed")!.Value, Is.EqualTo("9"));
        Assert.That(manager.Get("a", "renamed")!.Modified, Is.True);
        Assert.That(manager.Get("b", "renamed"), Is.Null);
        Assert.That(manager.DeletedLines.All(l => l.Section == "renamed"), Is.True);
        Assert.That(manager.ContainsSection("sect"), Is.False);

        await manager.SaveAsync();
        Assert.That(File.ReadAllText(path), Is.EqualTo("[renamed]\na=9\n"));
    }

    [Test]
    public async Task RenameSection_FlagFollowsHeader()
    {
        string path = WriteSample("[s]\nx=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.RemoveSection("s");
        Assert.That(manager.RenameSection("s", "t"), Is.True);

        Assert.That(manager.IsSectionDeleted("t"), Is.True);
        Assert.That(manager.IsSectionDeleted("s"), Is.False);
    }

    [Test]
    public async Task RenameSection_RejectsInvalidTargets()
    {
        string path = WriteSample("[s]\nx=1\n[t]\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.RemoveSection("t");

        Assert.That(manager.RenameSection("s", "t"), Is.False);
        Assert.That(manager.RenameSection("s", ""), Is.False);
        Assert.That(manager.RenameSection("s", "   "), Is.False);
        Assert.That(manager.RenameSection("s", "s"), Is.False);
        Assert.That(manager.RenameSection("missing", "z"), Is.False);
        Assert.That(manager.RenameSection("s", MpvConfManager.DefaultSectionName), Is.False);

        Assert.That(manager.ContainsSection("z"), Is.False);
        Assert.That(manager.Get("x", "s"), Is.Not.Null);
    }
}
