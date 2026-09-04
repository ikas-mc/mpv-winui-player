using mpv_winui.Modules.FileSystem;

namespace mpv_conf_test;

[TestFixture]
public class FileServiceTests
{
    private string _root = null!;
    private string _backupRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "mpv-fileservice-test-" + Guid.NewGuid().ToString("N"));
        AppData.Root = _root;
        _backupRoot = AppData.Current.ResolveLocalData("backup");
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

    private string WriteTarget(string content, string name)
    {
        var path = Path.Combine(_root, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Test]
    public async Task BackAndSave_BackupFalse_WritesOnlyFile_NoBackupCreated()
    {
        var path = WriteTarget("v0", "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: false);

        Assert.That(File.ReadAllText(path), Is.EqualTo("v1"));
        Assert.That(Directory.Exists(_backupRoot), Is.False);
    }

    [Test]
    public async Task BackAndSave_BackupTrue_BackupHoldsPreviousContent()
    {
        var path = WriteTarget("v0", "conf.conf");

        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v2", backup: true);

        Assert.That(File.ReadAllText(path), Is.EqualTo("v2"));
        var backups = Directory.GetFiles(Path.Combine(_backupRoot, "conf.conf"));
        Assert.That(backups, Has.Length.EqualTo(2));
        var contents = backups.Select(File.ReadAllText).ToList();
        Assert.That(contents, Does.Contain("v0"));
        Assert.That(contents, Does.Contain("v1"));
        Assert.That(contents, Does.Not.Contain("v2"));
    }

    [Test]
    public async Task BackAndSave_BackupTrue_WhenFileMissing_WritesWithoutBackup()
    {
        var path = Path.Combine(_root, "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "first", backup: true);

        Assert.That(File.ReadAllText(path), Is.EqualTo("first"));
        Assert.That(Directory.Exists(_backupRoot), Is.False);
    }

    [Test]
    public async Task BackupFileName_ContainsDashSeparatedMillisTimestamp()
    {
        var path = WriteTarget("v0", "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true);

        var backup = Directory.GetFiles(Path.Combine(_backupRoot, "conf.conf")).Single();
        var name = Path.GetFileName(backup);
        Assert.That(name, Does.Match(@"^conf\.\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}-\d{3}\.conf$"));
    }

    [Test]
    public async Task BackupFolder_NamedAfterFileWithoutExtension()
    {
        var path = WriteTarget("v0", "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true);

        Assert.That(Directory.Exists(Path.Combine(_backupRoot, "conf.conf")), Is.True);
    }

    [Test]
    public async Task BackupFolder_NoExtension_AppendsBak()
    {
        var path = WriteTarget("v0", "menu");
        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true);

        Assert.That(Directory.Exists(Path.Combine(_backupRoot, "menu")), Is.True);
    }

    [Test]
    public async Task Limit_Overflow_DeletesOldestByCreationTime()
    {
        var path = WriteTarget("v0", "conf.conf");

        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true, limit: 3);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v2", backup: true, limit: 3);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v3", backup: true, limit: 3);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v4", backup: true, limit: 3);

        Assert.That(File.ReadAllText(path), Is.EqualTo("v4"));
        var backups = Directory.GetFiles(Path.Combine(_backupRoot, "conf.conf"));
        Assert.That(backups, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task Limit_Zero_KeepsAllBackups()
    {
        var path = WriteTarget("v0", "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true, limit: 0);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v2", backup: true, limit: 0);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v3", backup: true, limit: 0);

        var backups = Directory.GetFiles(Path.Combine(_backupRoot, "conf.conf"));
        Assert.That(backups, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task BytesOverload_WritesUtf8Bytes()
    {
        var path = Path.Combine(_root, "binary.dat");
        var content = Encoding.UTF8.GetBytes("hwdec=auto\nlog-file=中文路径\n");
        await FileService.Instance.BackAndSaveAsync(path, content, backup: false);

        Assert.That(File.ReadAllBytes(path), Is.EqualTo(content));
        Assert.That(File.ReadAllText(path), Is.EqualTo("hwdec=auto\nlog-file=中文路径\n"));
    }

    [Test]
    public async Task StringOverload_WritesUtf8WithoutBom()
    {
        var path = Path.Combine(_root, "conf.conf");
        await FileService.Instance.BackAndSaveAsync(path, "选项=值\n", backup: false);

        var bytes = File.ReadAllBytes(path);
        Assert.That(bytes.Take(3), Is.Not.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.That(File.ReadAllText(path), Is.EqualTo("选项=值\n"));
    }

    [Test]
    public async Task Read_RoundTripsString()
    {
        var path = Path.Combine(_root, "conf.conf");
        File.WriteAllText(path, "a=1\nb=2\n", Encoding.UTF8);

        Assert.That(await FileService.Instance.ReadAsync(path), Is.EqualTo("a=1\nb=2\n"));
    }

    [Test]
    public async Task ReadAllLines_RoundTrips()
    {
        var path = Path.Combine(_root, "conf.conf");
        File.WriteAllText(path, "a=1\nb=2\nc=3\n", Encoding.UTF8);

        var lines = await FileService.Instance.ReadAllLinesAsync(path);
        Assert.That(lines, Is.EqualTo(new[] { "a=1", "b=2", "c=3" }));
    }

    [Test]
    public async Task Backup_ChineseFileName_BacksUpPreviousVersion()
    {
        var path = Path.Combine(_root, "管理 中文.conf");
        File.WriteAllText(path, "v0", Encoding.UTF8);

        await FileService.Instance.BackAndSaveAsync(path, "v1", backup: true, limit: 50);
        await Task.Delay(20);
        await FileService.Instance.BackAndSaveAsync(path, "v2", backup: true, limit: 50);

        Assert.That(File.ReadAllText(path), Is.EqualTo("v2"));
        var folder = Directory.GetDirectories(_backupRoot).Single();
        var backups = Directory.GetFiles(folder);
        var contents = backups.Select(File.ReadAllText).ToList();
        Assert.That(contents, Does.Contain("v0"));
        Assert.That(contents, Does.Contain("v1"));
    }
}
