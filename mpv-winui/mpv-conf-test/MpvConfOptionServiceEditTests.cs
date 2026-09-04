using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Option;
using mpv_winui.Modules.MpvConf.Schema;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfOptionServiceEditTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-conf-editor-test-" + Guid.NewGuid().ToString("N"));
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

    private MpvConfOptionService Editor(string text, out MpvConfManager manager)
    {
        string path = Path.Combine(_dir, "mpv.conf");
        File.WriteAllText(path, text);
        manager = new MpvConfManager(path);
        manager.Load();
        return new MpvConfOptionService(manager, MpvConfSchema.Empty);
    }

    private static MpvConfSchemaItem BoolDef() => MpvConfSchemaService.LoadFromJson("""
        [
          { "name": "key", "types": [{ "type": "bool" }] },
          { "name": "num", "types": [{ "type": "int" }] },
          { "name": "txt", "types": [{ "type": "string" }] }
        ]
        """).Get("key")!;

    [Test]
    public void ToggleCycle_ProducesSingleLine()
    {
        var editor = Editor("[mpvw-sdr]\nprofile-cond=x\nkey=yes\n", out var manager);
        var item = new MpvConfOptionItem("mpvw-sdr", BoolDef(), manager.Get("key", "mpvw-sdr"));

        editor.SetState(item, MpvOptionState.Disabled);
        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetState(item, MpvOptionState.Enabled);
        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.GetAll("key", "mpvw-sdr"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task EnableFromNotInFile_InsertsIntoSection_NotAtEndOfFile()
    {
        var editor = Editor("[s1]\na=1\n[s2]\nb=2\n", out var manager);
        var item = new MpvConfOptionItem("s1", BoolDef(), null);

        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(await Join(manager), Is.EqualTo("[s1]\na=1\nkey=\"\"\n[s2]\nb=2\n"));
    }

    [Test]
    public async Task EnableFromNotInFile_DefaultProfile_GoesBeforeFirstSection()
    {
        var editor = Editor("fs=yes\n[mpvw-sdr]\nprofile-cond=x\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(await Join(manager), Is.EqualTo("fs=yes\nkey=\"\"\n[mpvw-sdr]\nprofile-cond=x\n"));
    }

    [Test]
    public async Task Remove_RemovesOnlyOwnLine_KeepsOtherDuplicates()
    {
        var editor = Editor("[s]\nkey=yes\nkey=dup\nnum=3\n", out var manager);
        var item = new MpvConfOptionItem("s", BoolDef(), manager.Get("key", "s"));

        editor.SetState(item, MpvOptionState.NotInFile);

        Assert.That(manager.GetAll("key", "s"), Has.Count.EqualTo(1));
        Assert.That(manager.GetAll("key", "s")[0].Value, Is.EqualTo("dup"));
        Assert.That(await Join(manager), Is.EqualTo("[s]\nkey=dup\nnum=3\n"));
    }

    [Test]
    public async Task EnableFromNotInFile_KeepsExistingDuplicateLines()
    {
        var editor = Editor("[s]\nkey=old1\nkey=old2\n", out var manager);
        // An item reporting "not present" while duplicate lines already exist.
        var item = new MpvConfOptionItem("s", BoolDef(), null);

        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.GetAll("key", "s"), Has.Count.EqualTo(3));
        Assert.That(await Join(manager), Is.EqualTo("[s]\nkey=old1\nkey=old2\nkey=\"\"\n"));
    }

    [Test]
    public async Task EditOneDuplicate_DoesNotAffectOtherLines()
    {
        var editor = Editor("key=1\nkey=2\n", out var manager);
        var second = new MpvConfOptionItem("", BoolDef(), manager.GetAll("key", "")[1]);

        editor.SetValue(second, "3");
        await manager.SaveAsync();

        var lines = manager.GetAll("key", "");
        Assert.That(lines[0].Value, Is.EqualTo("1"));
        Assert.That(lines[1].Value, Is.EqualTo("3"));
    }

    [Test]
    public void EnableFromNotInFile_NoEdit_WritesDefinitionDefault()
    {
        var editor = Editor("other=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        // The user never touched the value; the definition default is empty.
        editor.SetState(item, MpvOptionState.Enabled);

        var line = manager.Get("key", "");
        Assert.That(line, Is.Not.Null);
        Assert.That(line!.Value, Is.EqualTo(""));
    }

    [Test]
    public void EnableFromNotInFile_WritesDefinitionDefault()
    {
        var editor = Editor("other=1\n", out var manager);
        var def = MpvConfSchemaService.LoadFromJson("""
            [
              { "name": "key", "default": "windy", "types": [{ "type": "string" }] }
            ]
            """).Get("key")!;
        var item = new MpvConfOptionItem("", def, null);

        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("windy"));
    }

    [Test]
    public void EnableFromNotInFile_UsesPendingValue()
    {
        var editor = Editor("other=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        editor.SetValue(item, "no"); // user edited the value while not in the file
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("no"));
    }

    [Test]
    public void DisableFromNotInFile_WritesValueAndComments()
    {
        var editor = Editor("other=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        editor.SetState(item, MpvOptionState.Disabled);

        var line = manager.Get("key", "");
        Assert.That(line, Is.Not.Null);
        Assert.That(line!.Value, Is.EqualTo(""));
        Assert.That(line.Enabled, Is.False);
    }

    [Test]
    public void EnableToDisable_KeepsValue()
    {
        var editor = Editor("key=no\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.Disabled);

        var line = manager.Get("key", "");
        Assert.That(line!.Value, Is.EqualTo("no"));
        Assert.That(line.Enabled, Is.False);
    }

    [Test]
    public void DisableToEnable_KeepsValue()
    {
        var editor = Editor("# key=no\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.Enabled);

        var line = manager.Get("key", "");
        Assert.That(line!.Value, Is.EqualTo("no"));
        Assert.That(line.Enabled, Is.True);
    }

    [Test]
    public void EnableToNotInFile_RemovesLine()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);

        Assert.That(manager.Get("key", ""), Is.Null);
    }

    [Test]
    public void NotInFileAfterValueEdit_ClearsPendingValue()
    {
        var editor = Editor("other=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        editor.SetValue(item, "no"); // pending edit while not in the file
        editor.SetState(item, MpvOptionState.Enabled); // writes "no", pending is retained
        editor.SetState(item, MpvOptionState.NotInFile); // RemoveLine must clear the pending value
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo(""));
    }

    [Test]
    public void DeleteExisting_IsModified_NotPresent()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);

        Assert.That(item.IsModified, Is.True);
        Assert.That(item.Present, Is.False);
        Assert.That(item.State, Is.EqualTo(MpvOptionState.NotInFile));
        Assert.That(manager.DeletedLines, Has.Count.EqualTo(1));
    }

    [Test]
    public void DeleteExisting_ValueReturnsTombstonedValue()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);

        Assert.That(item.Value, Is.EqualTo("yes"));
    }

    [Test]
    public void DeleteThenEditValue_PendingValueWinsOverTombstone()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetValue(item, "no");

        Assert.That(item.Value, Is.EqualTo("no"));
    }

    [Test]
    public void DeleteThenReEnable_ReturnsToExisting_NetZero()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(item.IsModified, Is.False);
        Assert.That(item.Present, Is.True);
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("yes"));
        Assert.That(manager.Get("key", "")!.Status, Is.EqualTo(MpvConfLineStatus.Existing));
        Assert.That(manager.Get("key", "")!.Modified, Is.False);
    }

    [Test]
    public void DeleteEditedThenReEnable_StillModified()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetValue(item, "a#b");
        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(item.IsModified, Is.True);
        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("a#b"));
    }

    [Test]
    public void DeleteThenEditValueThenReEnable_AppliesPendingValue()
    {
        var editor = Editor("key=yes\nother=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), manager.Get("key", ""));

        editor.SetState(item, MpvOptionState.NotInFile);
        editor.SetValue(item, "no");
        editor.SetState(item, MpvOptionState.Enabled);

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("no"));
        Assert.That(item.IsModified, Is.True);
    }

    [Test]
    public void AddThenDelete_NetZero()
    {
        var editor = Editor("other=1\n", out var manager);
        var item = new MpvConfOptionItem("", BoolDef(), null);

        editor.SetState(item, MpvOptionState.Enabled);
        editor.SetState(item, MpvOptionState.NotInFile);

        Assert.That(item.IsModified, Is.False);
        Assert.That(item.Present, Is.False);
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("key", ""), Is.Null);
    }

    private static async Task<string> Join(MpvConfManager manager)
    {
        await manager.SaveAsync();
        return string.Join("\n", manager.Lines.Select(l => l.Raw)) + "\n";
    }
}
