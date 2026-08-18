using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Option;
using mpv_winui.Modules.MpvConf.Schema;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfOptionItemTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-conf-item-test-" + Guid.NewGuid().ToString("N"));
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

    private MpvConfManager Manager(string text)
    {
        string path = Path.Combine(_dir, "mpv.conf");
        File.WriteAllText(path, text);
        var manager = new MpvConfManager(path);
        manager.Load();
        return manager;
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
        var manager = Manager("[mpvw-sdr]\nprofile-cond=x\nkey=yes\n");
        var item = new MpvConfOptionItem(manager, "mpvw-sdr", BoolDef(), manager.Get("key", "mpvw-sdr"));

        item.State = MpvOptionState.Disabled;
        item.State = MpvOptionState.NotInFile;
        item.State = MpvOptionState.Enabled;
        item.State = MpvOptionState.NotInFile;
        item.State = MpvOptionState.Enabled;

        Assert.That(manager.GetAll("key", "mpvw-sdr"), Has.Count.EqualTo(1));
    }

    [Test]
    public void EnableFromNotInFile_InsertsIntoSection_NotAtEndOfFile()
    {
        var manager = Manager("[s1]\na=1\n[s2]\nb=2\n");
        var item = new MpvConfOptionItem(manager, "s1", BoolDef(), null);

        item.State = MpvOptionState.Enabled;

        Assert.That(Join(manager), Is.EqualTo("[s1]\na=1\nkey=\"\"\n[s2]\nb=2\n"));
    }

    [Test]
    public void EnableFromNotInFile_DefaultProfile_GoesBeforeFirstSection()
    {
        var manager = Manager("fs=yes\n[mpvw-sdr]\nprofile-cond=x\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        item.State = MpvOptionState.Enabled;

        Assert.That(Join(manager), Is.EqualTo("fs=yes\nkey=\"\"\n[mpvw-sdr]\nprofile-cond=x\n"));
    }

    [Test]
    public void Remove_RemovesOnlyOwnLine_KeepsOtherDuplicates()
    {
        var manager = Manager("[s]\nkey=yes\nkey=dup\nnum=3\n");
        var item = new MpvConfOptionItem(manager, "s", BoolDef(), manager.Get("key", "s"));

        item.State = MpvOptionState.NotInFile;

        Assert.That(manager.GetAll("key", "s"), Has.Count.EqualTo(1));
        Assert.That(manager.GetAll("key", "s")[0].Value, Is.EqualTo("dup"));
        Assert.That(Join(manager), Is.EqualTo("[s]\nkey=dup\nnum=3\n"));
    }

    [Test]
    public void EnableFromNotInFile_KeepsExistingDuplicateLines()
    {
        var manager = Manager("[s]\nkey=old1\nkey=old2\n");
        // An item reporting "not present" while duplicate lines already exist.
        var item = new MpvConfOptionItem(manager, "s", BoolDef(), null);

        item.State = MpvOptionState.Enabled;

        Assert.That(manager.GetAll("key", "s"), Has.Count.EqualTo(3));
        Assert.That(Join(manager), Is.EqualTo("[s]\nkey=old1\nkey=old2\nkey=\"\"\n"));
    }

    [Test]
    public void EditOneDuplicate_DoesNotAffectOtherLines()
    {
        var manager = Manager("key=1\nkey=2\n");
        var second = new MpvConfOptionItem(manager, "", BoolDef(), manager.GetAll("key", "")[1]);

        second.Value = "3";
        manager.Save();

        var lines = manager.GetAll("key", "");
        Assert.That(lines[0].Value, Is.EqualTo("1"));
        Assert.That(lines[1].Value, Is.EqualTo("3"));
    }

    [Test]
    public void EnableFromNotInFile_NoEdit_WritesDefinitionDefault()
    {
        var manager = Manager("other=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        // The user never touched the value; the definition default is empty.
        item.State = MpvOptionState.Enabled;

        var line = manager.Get("key", "");
        Assert.That(line, Is.Not.Null);
        Assert.That(line!.Value, Is.EqualTo(""));
    }

    [Test]
    public void EnableFromNotInFile_WritesDefinitionDefault()
    {
        var manager = Manager("other=1\n");
        var def = MpvConfSchemaService.LoadFromJson("""
            [
              { "name": "key", "default": "windy", "types": [{ "type": "string" }] }
            ]
            """).Get("key")!;
        var item = new MpvConfOptionItem(manager, "", def, null);

        item.State = MpvOptionState.Enabled;

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("windy"));
    }

    [Test]
    public void EnableFromNotInFile_UsesPendingValue()
    {
        var manager = Manager("other=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        item.Value = "no"; // user edited the value while not in the file
        item.State = MpvOptionState.Enabled;

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("no"));
    }

    [Test]
    public void DisableFromNotInFile_WritesValueAndComments()
    {
        var manager = Manager("other=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        item.State = MpvOptionState.Disabled;

        var line = manager.Get("key", "");
        Assert.That(line, Is.Not.Null);
        Assert.That(line!.Value, Is.EqualTo(""));
        Assert.That(line.Enabled, Is.False);
    }

    [Test]
    public void EnableToDisable_KeepsValue()
    {
        var manager = Manager("key=no\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.Disabled;

        var line = manager.Get("key", "");
        Assert.That(line!.Value, Is.EqualTo("no"));
        Assert.That(line.Enabled, Is.False);
    }

    [Test]
    public void DisableToEnable_KeepsValue()
    {
        var manager = Manager("# key=no\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.Enabled;

        var line = manager.Get("key", "");
        Assert.That(line!.Value, Is.EqualTo("no"));
        Assert.That(line.Enabled, Is.True);
    }

    [Test]
    public void EnableToNotInFile_RemovesLine()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;

        Assert.That(manager.Get("key", ""), Is.Null);
    }

    [Test]
    public void NotInFileAfterValueEdit_ClearsPendingValue()
    {
        var manager = Manager("other=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        item.Value = "no"; // pending edit while not in the file
        item.State = MpvOptionState.Enabled; // writes "no", pending is retained
        item.State = MpvOptionState.NotInFile; // RemoveLine must clear the pending value
        item.State = MpvOptionState.Enabled;

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo(""));
    }

    [Test]
    public void DeleteExisting_IsModified_NotPresent()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;

        Assert.That(item.IsModified, Is.True);
        Assert.That(item.Present, Is.False);
        Assert.That(item.State, Is.EqualTo(MpvOptionState.NotInFile));
        Assert.That(manager.DeletedLines, Has.Count.EqualTo(1));
    }

    [Test]
    public void DeleteExisting_ValueReturnsTombstonedValue()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;

        Assert.That(item.Value, Is.EqualTo("yes"));
    }

    [Test]
    public void DeleteThenEditValue_PendingValueWinsOverTombstone()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;
        item.Value = "no";

        Assert.That(item.Value, Is.EqualTo("no"));
    }

    [Test]
    public void DeleteThenReEnable_ReturnsToExisting_NetZero()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;
        item.State = MpvOptionState.Enabled;

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
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.Value = "a#b";
        item.State = MpvOptionState.NotInFile;
        item.State = MpvOptionState.Enabled;

        Assert.That(item.IsModified, Is.True);
        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("a#b"));
    }

    [Test]
    public void DeleteThenEditValueThenReEnable_AppliesPendingValue()
    {
        var manager = Manager("key=yes\nother=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), manager.Get("key", ""));

        item.State = MpvOptionState.NotInFile;
        item.Value = "no";
        item.State = MpvOptionState.Enabled;

        Assert.That(manager.Get("key", "")!.Value, Is.EqualTo("no"));
        Assert.That(item.IsModified, Is.True);
    }

    [Test]
    public void AddThenDelete_NetZero()
    {
        var manager = Manager("other=1\n");
        var item = new MpvConfOptionItem(manager, "", BoolDef(), null);

        item.State = MpvOptionState.Enabled;
        item.State = MpvOptionState.NotInFile;

        Assert.That(item.IsModified, Is.False);
        Assert.That(item.Present, Is.False);
        Assert.That(manager.DeletedLines, Is.Empty);
        Assert.That(manager.Get("key", ""), Is.Null);
    }

    private static string Join(MpvConfManager manager)
    {
        manager.Save();
        return string.Join("\n", manager.Lines.Select(l => l.Raw)) + "\n";
    }
}
