using mpv_winui.Modules.MpvConf.Conf;
using mpv_winui.Modules.MpvConf.Option;
using mpv_winui.Modules.MpvConf.Schema;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfOptionServiceTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-conf-listing-test-" + Guid.NewGuid().ToString("N"));
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

    private static MpvConfSchema Schema() => MpvConfSchemaService.LoadFromJson("""
        [
          { "name": "alpha", "group": "video", "types": [{ "type": "string" }] },
          { "name": "beta",  "group": "audio", "types": [{ "type": "int" }] },
          { "name": "gamma", "group": "video", "types": [{ "type": "bool" }] }
        ]
        """);

    private static List<MpvConfOptionItem> Build(MpvConfManager m, string profile, string? group, MpvConfOptionIncludeType mode) =>
        MpvConfOptionService.GetOptions(m, Schema(), profile, group, mode).ToList();

    [Test]
    public void Sections_ReturnsOrderedDistinctSections()
    {
        var m = Manager("[s1]\na=1\n[s2]\nb=2\n[s1]\nc=3\n");
        Assert.That(m.Sections, Is.EqualTo(new[] { "s1", "s2" }));
    }

    [Test]
    public void Build_AllMode_IncludesOptionsNotInFile()
    {
        var m = Manager("alpha=auto\nx=1\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.All);

        Assert.That(result.Select(r => r.Key),
            Is.EquivalentTo(new[] { "alpha", "beta", "gamma", "x" }));
        Assert.That(result.Single(r => r.Key == "beta").Present, Is.False);
        Assert.That(result.Single(r => r.Key == "x").Present, Is.True);
        Assert.That(result.Single(r => r.Key == "x").Definition, Is.Null);
    }

    [Test]
    public void Build_AllMode_KeepsSchemaOrder()
    {
        var m = Manager("gamma=yes\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.All);

        Assert.That(result.Select(r => r.Key), Is.EqualTo(new[] { "alpha", "beta", "gamma" }));
    }

    [Test]
    public void Build_FromConfig_ExcludesNotInFile()
    {
        var m = Manager("alpha=auto\nx=1\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.FromConfig);

        Assert.That(result.Select(r => r.Key), Is.EquivalentTo(new[] { "alpha", "x" }));
    }

    [Test]
    public void Build_Effective_OnlyEnabled()
    {
        var m = Manager("alpha=auto\n# beta=2\ngamma=yes\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.Effective);

        Assert.That(result.Select(r => r.Key), Is.EquivalentTo(new[] { "alpha", "gamma" }));
    }

    [Test]
    public void Build_GroupFilter_OnlyThatGroup()
    {
        var m = Manager("alpha=auto\nbeta=2\ngamma=yes\n");
        var result = Build(m, "", "audio", MpvConfOptionIncludeType.All);

        Assert.That(result.Select(r => r.Key), Is.EqualTo(new[] { "beta" }));
    }

    [Test]
    public void Build_UnknownGroup_OnlyUnknownOptions()
    {
        var m = Manager("alpha=auto\nx=1\ny=2\n");
        var result = Build(m, "", MpvConfOptionService.UnknownGroup, MpvConfOptionIncludeType.All);

        Assert.That(result.Select(r => r.Key).OrderBy(k => k), Is.EqualTo(new[] { "x", "y" }));
        Assert.That(result.All(r => r.Definition is null), Is.True);
    }

    [Test]
    public void Build_AllMode_DuplicateKeyLines_ProduceOneEntryPerLine()
    {
        var m = Manager("alpha=a\nalpha=b\nbeta=2\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.All);

        var alphas = result.Where(r => r.Key == "alpha").ToList();
        Assert.That(alphas, Has.Count.EqualTo(2));
        Assert.That(alphas.Select(r => r.Line!.Value), Is.EqualTo(new[] { "a", "b" }));
        Assert.That(alphas.All(r => r.Present), Is.True);
    }

    [Test]
    public void Build_FromConfig_DuplicateKeyLines_AllPresent()
    {
        var m = Manager("alpha=a\n# alpha=b\nbeta=2\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.FromConfig);

        var alphas = result.Where(r => r.Key == "alpha").ToList();
        Assert.That(alphas, Has.Count.EqualTo(2));
        Assert.That(alphas[1].Line!.Enabled, Is.False);
    }

    [Test]
    public void Build_Effective_DisabledDuplicateExcluded()
    {
        var m = Manager("alpha=a\n# alpha=b\n");
        var result = Build(m, "", null, MpvConfOptionIncludeType.Effective);

        Assert.That(result.Where(r => r.Key == "alpha").ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    public void Build_ProfileScoping_IgnoresOtherProfiles()
    {
        var m = Manager("alpha=auto\n[s1]\nalpha=manual\nbeta=5\n");

        var defaultResult = Build(m, "", null, MpvConfOptionIncludeType.FromConfig);
        Assert.That(defaultResult.Select(r => r.Key), Does.Not.Contain("beta"));

        var s1Result = Build(m, "s1", null, MpvConfOptionIncludeType.FromConfig);
        Assert.That(s1Result.Single(r => r.Key == "beta").Present, Is.True);
        Assert.That(s1Result.Single(r => r.Key == "alpha").Line!.Value, Is.EqualTo("manual"));
    }

    [Test]
    public void HasUnknownOptions_IsProfileSpecific()
    {
        var m = Manager("x=1\n[s1]\nalpha=auto\n");
        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "", MpvConfOptionIncludeType.All), Is.True);
        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "s1", MpvConfOptionIncludeType.All), Is.False);
    }

    [Test]
    public void AvailableGroups_AllMode_ReturnsSchemaGroupsInAppearanceOrder()
    {
        var m = Manager("alpha=auto\n");
        Assert.That(MpvConfOptionService.GetGroups(m, Schema(), "", MpvConfOptionIncludeType.All),
            Is.EqualTo(new[] { "video", "audio" }));
    }

    [Test]
    public void AvailableGroups_FromConfig_OnlyGroupsWithPresentOptions()
    {
        var m = Manager("alpha=auto\nx=1\n");
        var groups = MpvConfOptionService.GetGroups(m, Schema(), "", MpvConfOptionIncludeType.FromConfig);

        Assert.That(groups, Is.EqualTo(new[] { "video", MpvConfOptionService.UnknownGroup }));
    }

    [Test]
    public void AvailableGroups_Effective_OnlyGroupsWithEnabledOptions()
    {
        var m = Manager("alpha=auto\n# beta=2\ngamma=yes\n");
        var groups = MpvConfOptionService.GetGroups(m, Schema(), "", MpvConfOptionIncludeType.Effective);

        Assert.That(groups, Is.EqualTo(new[] { "video" }));
    }

    [Test]
    public void HasUnknownOptions_EffectiveMode_ExcludesDisabledUnknown()
    {
        var m = Manager("# x=1\n");
        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "", MpvConfOptionIncludeType.FromConfig), Is.True);
        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "", MpvConfOptionIncludeType.Effective), Is.False);
    }

    [Test]
    public void Build_Modified_IncludesDeletedKnownOption()
    {
        var m = Manager("alpha=auto\nbeta=2\n");
        m.Remove(m.Get("alpha")!);

        var result = Build(m, "", null, MpvConfOptionIncludeType.Modified);
        var deleted = result.Single(r => r.Key == "alpha");

        Assert.That(deleted.Present, Is.False);
        Assert.That(deleted.IsModified, Is.True);
        Assert.That(result.Select(r => r.Key), Does.Not.Contain("beta"));
    }

    [Test]
    public void Build_Modified_IncludesDeletedUnknownOption()
    {
        var m = Manager("alpha=auto\nx=1\n");
        m.Remove(m.Get("x")!);

        var result = Build(m, "", null, MpvConfOptionIncludeType.Modified);
        var deleted = result.Single(r => r.Key == "x");

        Assert.That(deleted.Definition, Is.Null);
        Assert.That(deleted.IsModified, Is.True);
        Assert.That(deleted.Present, Is.False);
    }

    [Test]
    public void Build_OtherModes_ExcludeDeleted()
    {
        var m = Manager("alpha=auto\n");
        m.Remove(m.Get("alpha")!);

        Assert.That(Build(m, "", null, MpvConfOptionIncludeType.FromConfig).Select(r => r.Key), Does.Not.Contain("alpha"));
        Assert.That(Build(m, "", null, MpvConfOptionIncludeType.Effective).Select(r => r.Key), Does.Not.Contain("alpha"));

        var all = Build(m, "", null, MpvConfOptionIncludeType.All).Single(r => r.Key == "alpha");
        Assert.That(all.Present, Is.False);
        Assert.That(all.IsModified, Is.False);
    }

    [Test]
    public void AvailableGroups_Modified_KeepsDeletedGroup()
    {
        var m = Manager("alpha=auto\n");
        m.Remove(m.Get("alpha")!);

        var groups = MpvConfOptionService.GetGroups(m, Schema(), "", MpvConfOptionIncludeType.Modified);
        Assert.That(groups, Does.Contain("video"));
    }

    [Test]
    public void ContainsUnknownOptions_Modified_CountsDeletedUnknown()
    {
        var m = Manager("x=1\n");
        m.Remove(m.Get("x")!);

        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "", MpvConfOptionIncludeType.Modified), Is.True);
        Assert.That(MpvConfOptionService.ContainsUnknownOptions(m, Schema(), "", MpvConfOptionIncludeType.All), Is.False);
    }
}
