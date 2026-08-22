using mpv_winui.Modules.MpvConf.Schema;
namespace mpv_conf_test;

[TestFixture]
public class MpvConfSchemaServiceTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mpv-schema-service-test-" + Guid.NewGuid().ToString("N"));
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

    private string WriteSchema(string name, string json)
    {
        string path = Path.Combine(_dir, name);
        File.WriteAllText(path, json);
        return path;
    }

    [Test]
    public void LoadFromJson_ParsesBasicDefinition()
    {
        const string json = """
        [
          {
            "name": "hwdec",
            "group": "video",
            "desc": "Hardware decoding",
            "link": "https://mpv.io/manual/master/#options-hwdec",
            "values": [ { "type": "string", "enum": [ {"value":"auto","desc":"Auto"}, {"value":"yes","desc":"Yes"}, {"value":"no","desc":"No"} ] } ]
          }
        ]
        """;

        var schema = MpvConfSchemaService.LoadFromJson(json);

        Assert.That(schema.Count, Is.EqualTo(1));
        var def = schema.Get("hwdec")!;
        Assert.That(def.Name, Is.EqualTo("hwdec"));
        Assert.That(def.Group, Is.EqualTo("video"));
        Assert.That(def.Description, Is.EqualTo("Hardware decoding"));
        Assert.That(def.Link, Is.EqualTo("https://mpv.io/manual/master/#options-hwdec"));
        Assert.That(def.Values, Has.Count.EqualTo(1));
        Assert.That(def.Values[0].Type, Is.EqualTo("string"));
        Assert.That(def.Values[0].EnumValues, Has.Count.EqualTo(3));
        Assert.That(def.Values[0].EnumValues![0].Value, Is.EqualTo("auto"));
        Assert.That(def.Values[0].EnumValues![0].Desc, Is.EqualTo("Auto"));
        Assert.That(def.Values[0].EnumValues![1].Value, Is.EqualTo("yes"));
        Assert.That(def.Values[0].EnumValues![1].Desc, Is.EqualTo("Yes"));
        Assert.That(def.Values[0].EnumValues![2].Value, Is.EqualTo("no"));
        Assert.That(def.Values[0].EnumValues![2].Desc, Is.EqualTo("No"));
    }

    [Test]
    public void LoadFromJson_ParsesNumericBounds()
    {
        const string json = """
        [ { "name": "volume-max", "values": [ { "type": "int", "minimum": 0, "maximum": 200 } ] } ]
        """;

        var def = MpvConfSchemaService.LoadFromJson(json).Get("volume-max")!;
        Assert.That(def.Values[0].Minimum, Is.EqualTo(0));
        Assert.That(def.Values[0].Maximum, Is.EqualTo(200));
    }

    [Test]
    public void LoadFromJson_MissingTypesDefaultsToRaw()
    {
        const string json = """[ { "name": "foo", "desc": "x" } ]""";
        var def = MpvConfSchemaService.LoadFromJson(json).Get("foo")!;
        Assert.That(def.Values, Has.Count.EqualTo(1));
        Assert.That(def.Values[0].Type, Is.EqualTo("raw"));
    }

    [Test]
    public void LoadFromJson_EmptyTypesDefaultsToRaw()
    {
        const string json = """[ { "name": "foo", "values": [] } ]""";
        var def = MpvConfSchemaService.LoadFromJson(json).Get("foo")!;
        Assert.That(def.Values, Has.Count.EqualTo(1));
        Assert.That(def.Values[0].Type, Is.EqualTo("raw"));
    }

    [Test]
    public void LoadFromJson_MissingGroupDefaultsToGeneral()
    {
        const string json = """[ { "name": "foo", "values": [ { "type": "bool" } ] } ]""";
        var def = MpvConfSchemaService.LoadFromJson(json).Get("foo")!;
        Assert.That(def.Group, Is.EqualTo("General"));
    }

    [Test]
    public void LoadFromJson_UnknownKeyReturnsNull()
    {
        const string json = """[ { "name": "foo", "values": [ { "type": "bool" } ] } ]""";
        Assert.That(MpvConfSchemaService.LoadFromJson(json).Get("missing"), Is.Null);
    }

    [Test]
    public void LoadFromJson_MalformedThrowsJsonException()
    {
        Assert.That(() => MpvConfSchemaService.LoadFromJson("{ not json"), Throws.Exception.InstanceOf<JsonException>());
    }

    [Test]
    public void Merge_FirstDefinitionWins()
    {
        const string first = """[ { "name": "osc", "group": "a" } ]""";
        const string second = """[ { "name": "osc", "group": "b" } ]""";

        var merged = MpvConfSchemaService.Merge(
            MpvConfSchemaService.LoadFromJson(first),
            MpvConfSchemaService.LoadFromJson(second));

        Assert.That(merged.Get("osc")!.Group, Is.EqualTo("a"));
    }

    [Test]
    public void LoadFromFile_MissingFileReturnsEmpty()
    {
        var schema = MpvConfSchemaService.LoadFromFile(Path.Combine(_dir, "nope.json"));
        Assert.That(schema.Count, Is.EqualTo(0));
    }

    [Test]
    public void LoadFromDirectory_MergesMultipleFilesSortedAndSkipsBroken()
    {
        WriteSchema("b.json", """[ { "name": "bb", "group": "b" } ]""");
        WriteSchema("a.json", """[ { "name": "aa", "group": "a" } ]""");
        WriteSchema("broken.json", "{ not json");

        var schema = MpvConfSchemaService.LoadFromDirectory(_dir);

        Assert.That(schema.Count, Is.EqualTo(2));
        Assert.That(schema.Get("aa"), Is.Not.Null);
        Assert.That(schema.Get("bb"), Is.Not.Null);
    }

    [Test]
    public void LoadFromDirectory_FirstFileWinsOnOverlap()
    {
        WriteSchema("a.json", """[ { "name": "dup", "group": "a" } ]""");
        WriteSchema("b.json", """[ { "name": "dup", "group": "b" } ]""");

        var schema = MpvConfSchemaService.LoadFromDirectory(_dir);

        Assert.That(schema.Get("dup")!.Group, Is.EqualTo("a"));
    }

    [Test]
    public void LoadFromDirectory_MissingDirectoryReturnsEmpty()
    {
        var schema = MpvConfSchemaService.LoadFromDirectory(Path.Combine(_dir, "missing"));
        Assert.That(schema.Count, Is.EqualTo(0));
    }

    [Test]
    public void LoadFromJson_ParsesEnumWithDescriptionOnly()
    {
        const string json = """
        [
          {
            "name": "vo",
            "values": [ { "type": "string", "enum": [ {"value":"gpu","desc":"GPU"}, {"value":"vulkan"} ] } ]
          }
        ]
        """;

        var def = MpvConfSchemaService.LoadFromJson(json).Get("vo")!;
        Assert.That(def.Values[0].EnumValues, Has.Count.EqualTo(2));
        Assert.That(def.Values[0].EnumValues![0].Value, Is.EqualTo("gpu"));
        Assert.That(def.Values[0].EnumValues![0].Desc, Is.EqualTo("GPU"));
        Assert.That(def.Values[0].EnumValues![1].Value, Is.EqualTo("vulkan"));
        Assert.That(def.Values[0].EnumValues![1].Desc, Is.Null);
    }

    [Test]
    public void LoadFromJson_EmptyEnumList()
    {
        const string json = """[ { "name": "foo", "values": [ { "type": "string", "enum": [] } ] } ]""";
        var def = MpvConfSchemaService.LoadFromJson(json).Get("foo")!;
        Assert.That(def.Values[0].HasEnum, Is.False);
        Assert.That(def.Values[0].EnumValues, Is.Empty);
    }
}
