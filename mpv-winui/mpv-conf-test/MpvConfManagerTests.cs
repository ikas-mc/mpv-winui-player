using mpv_winui.Modules.MpvConf.Conf;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfManagerTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-conf-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown()
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
    public void LoadAndSave_RoundTripsUnchangedFile()
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
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test]
    public void ModifyValue_SavesOnlyThatLineChanged()
    {
        string path = WriteSample("msg-level=all=error\nlog-file=~~/mpv.log\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var option = manager.Get("msg-level")!;
        option.Value = "all=info";

        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("msg-level=all=info\nlog-file=~~/mpv.log\n"));
    }

    [Test]
    public void ToggleEnabled_AddsAndRemovesHash()
    {
        string path = WriteSample("osc=no\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var option = manager.Get("osc")!;
        option.Enabled = false;
        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("# osc=no\n"));

        manager.Load();
        var disabled = manager.Get("osc")!;
        Assert.That(disabled.Enabled, Is.False);
        disabled.Enabled = true;
        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("osc=no\n"));
    }

    [Test]
    public void InsertSection_AddsHeader_OnlyOnce()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertSection("newsec");
        manager.InsertSection("newsec");
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n[newsec]\n"));
        Assert.That(manager.Sections, Does.Contain("newsec"));
    }

    [Test]
    public void InsertOption_AppendsAtEndAndReindexes()
    {
        string path = WriteSample("a=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var added = manager.InsertOption("c", "3");
        Assert.That(added.LineNumber, Is.EqualTo(2));

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nb=2\nc=3\n"));

        var lines = manager.Lines;
        Assert.That(lines[0].LineNumber, Is.EqualTo(0));
        Assert.That(lines[2].LineNumber, Is.EqualTo(2));
    }

    [Test]
    public void InsertOption_InSection_AppendsAfterSectionLines()
    {
        string path = WriteSample("[sect]\na=1\nb=2\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var added = manager.InsertOption("c", "3", "sect");
        Assert.That(added.Section, Is.EqualTo("sect"));

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("[sect]\na=1\nb=2\nc=3\n"));
    }

    [Test]
    public void InsertOption_IntoMissingSection_CreatesHeader()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("c", "3", "newsection");
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n[newsection]\nc=3\n"));
    }

    [Test]
    public void UnknownAndBlankLinesArePreservedOnSave()
    {
        string path = WriteSample("# note\n\nweird line here\nreal=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("# note\n\nweird line here\nreal=1\n"));
        Assert.That(manager.Lines[2].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public void InsertDisabled_WritesCommentPrefix()
    {
        string path = WriteSample(string.Empty);
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertDisabled("gpu-api", "d3d11");
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("# gpu-api=d3d11\n"));
    }

    [Test]
    public void GetAll_ReturnsRepeatedKeys()
    {
        string path = WriteSample("key=a\nkey=b\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.GetAll("key"), Has.Count.EqualTo(2));
    }

    [Test]
    public void Load_ReadsLineBased_WithAndWithoutTrailingNewline()
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
    public void Load_EmptyFile_ProducesNoLines()
    {
        string path = WriteSample(string.Empty);
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Lines, Is.Empty);
        Assert.That(manager.IsLoaded, Is.True);
    }

    [Test]
    public void Load_MissingFile_ProducesNoLinesAndIsLoaded()
    {
        var manager = new MpvConfManager(Path.Combine(_dir, "missing.conf"));
        manager.Load();

        Assert.That(manager.Lines, Is.Empty);
        Assert.That(manager.IsLoaded, Is.True);
    }

    [Test]
    public void Load_CrLfFile_ReadsOptionsAndValueWithoutCr()
    {
        string path = WriteSample("a=1\r\nb=2\r\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Get("a")!.Value, Is.EqualTo("1"));
        Assert.That(manager.Get("b")!.Value, Is.EqualTo("2"));
        Assert.That(manager.Lines, Has.Count.EqualTo(2));
    }

    [Test]
    public void Load_Utf8Bom_IsStrippedByReader()
    {
        string path = Path.Combine(_dir, "bom.conf");
        File.WriteAllText(path, "\uFEFFhwdec=auto\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Get("hwdec")!.Value, Is.EqualTo("auto"));
    }

    [Test]
    public void Save_NormalizesToTrailingNewline()
    {
        string path = Path.Combine(_dir, "plain.conf");
        File.WriteAllText(path, "a=1");
        var manager = new MpvConfManager(path);
        manager.Load();
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n"));
    }

    [Test]
    public void Save_WritesLfLineEndings()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        manager.Save();

        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\n"));
        Assert.That(File.ReadAllText(path), Does.Not.Contain("\r"));
    }

    [Test]
    public void LoadAndSave_ReloadKeepsSameOptions()
    {
        string path = WriteSample("# note\n# osc=no\n[sec]\nprofile-cond=x\n");
        var manager = new MpvConfManager(path);
        manager.Load();
        manager.Save();

        var reloaded = new MpvConfManager(path);
        reloaded.Load();

        Assert.That(reloaded.Lines, Has.Count.EqualTo(manager.Lines.Count));
        Assert.That(reloaded.Get("osc")!.Enabled, Is.False);
        Assert.That(reloaded.Get("profile-cond", "sec")!.Value, Is.EqualTo("x"));
    }

    [Test]
    public void Remove_DeletesLineAndReindexes()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        Assert.That(manager.Remove(b), Is.True);
        Assert.That(manager.Get("b"), Is.Null);
        Assert.That(manager.Get("c")!.LineNumber, Is.EqualTo(1));

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nc=3\n"));
    }

    [Test]
    public void Remove_ReturnsFalseForForeignLine()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var other = new MpvConfManager(WriteSample("x=9\n"));
        other.Load();
        Assert.That(manager.Remove(other.Lines[0]), Is.False);
    }

    [Test]
    public void InsertOption_IntoDefaultProfile_GoesBeforeFirstSection()
    {
        string path = WriteSample("fs=yes\n[mpvw-sdr]\nprofile-cond=x\n[other]\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("hwdec", "auto", "");

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("fs=yes\nhwdec=auto\n[mpvw-sdr]\nprofile-cond=x\n[other]\n"));
    }

    [Test]
    public void InsertOption_IntoDefaultProfile_NoSections_AppendsAtEnd()
    {
        string path = WriteSample("fs=yes\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        manager.InsertOption("hwdec", "auto", "");

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("fs=yes\nhwdec=auto\n"));
    }

    [Test]
    public void Remove_ExistingLine_BecomesDeleted_NotVisible()
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
    public void Remove_AddedLine_DropsEntirely()
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
    public void Restore_DeletedLine_ReturnsToExisting_KeepsPosition()
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
    public void Restore_NonDeletedLine_ReturnsFalse()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        Assert.That(manager.Restore(manager.Get("a")!), Is.False);
    }

    [Test]
    public void Remove_AlreadyDeleted_ReturnsFalse()
    {
        string path = WriteSample("a=1\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var a = manager.Get("a")!;
        manager.Remove(a);
        Assert.That(manager.Remove(a), Is.False);
    }

    [Test]
    public void Save_RemovesDeletedLines_AndResetsStatus()
    {
        string path = WriteSample("a=1\nb=2\nc=3\n");
        var manager = new MpvConfManager(path);
        manager.Load();

        var b = manager.Get("b")!;
        manager.Remove(b);
        var added = manager.InsertOption("d", "4");

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nc=3\nd=4\n"));
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Lines, Does.Not.Contain(b));
        Assert.That(manager.Get("c")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("c")!.Modified, Is.False);
        Assert.That(manager.Get("d")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("d")!.Modified, Is.False);
    }

    [Test]
    public void InsertOption_AfterDeletion_RestoresInPlace()
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

        manager.Save();
        Assert.That(File.ReadAllText(path), Is.EqualTo("a=1\nb=9\n"));
    }
}
