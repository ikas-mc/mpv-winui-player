using mpv_winui.Modules.MpvConf.Option;
using mpv_winui.Modules.MpvConf.Schema;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfOptionHelperTests
{
    private static MpvConfSchemaItemValue Type(string type, MpvConfSchemaEnumValue[]? enumValues = null) =>
        new()
        {
            Type = type,
            EnumValues = enumValues,
        };

    [Test]
    public void ResolveEditorKind_NullType_IsText()
    {
        Assert.That(MpvConfOptionHelper.ResolveEditorKind(null), Is.EqualTo(MpvOptionEditorKind.Text));
    }

    [TestCase("bool", MpvOptionEditorKind.Bool)]
    [TestCase("int", MpvOptionEditorKind.Int)]
    [TestCase("float", MpvOptionEditorKind.Float)]
    [TestCase("string", MpvOptionEditorKind.Text)]
    [TestCase("array", MpvOptionEditorKind.Text)]
    [TestCase("raw", MpvOptionEditorKind.Text)]
    public void ResolveEditorKind_SingleType(string type, MpvOptionEditorKind expected)
    {
        Assert.That(MpvConfOptionHelper.ResolveEditorKind(Type(type)), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveEditorKind_WithEnum_IsEnum()
    {
        Assert.That(MpvConfOptionHelper.ResolveEditorKind(Type("string", [new() { Value = "auto" }, new() { Value = "yes" }])), Is.EqualTo(MpvOptionEditorKind.Enum));
    }

    [TestCase("yes", true)]
    [TestCase("YES", true)]
    [TestCase("no", false)]
    [TestCase("NO", false)]
    public void ParseBool_RecognizedWords(string input, bool expected)
    {
        Assert.That(MpvConfOptionHelper.ParseBool(input), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("maybe")]
    [TestCase("2")]
    [TestCase("enabled")]
    [TestCase("on")]
    [TestCase("1")]
    [TestCase("false")]
    [TestCase("disabled")]
    [TestCase("off")]
    [TestCase("0")]
    [TestCase("true")]
    public void ParseBool_Unrecognized_IsNull(string? input)
    {
        Assert.That(MpvConfOptionHelper.ParseBool(input), Is.Null);
    }

    [TestCase(true, "yes")]
    [TestCase(false, "no")]
    public void FormatBool(bool value, string expected)
    {
        Assert.That(MpvConfOptionHelper.FormatBool(value), Is.EqualTo(expected));
    }

    [TestCase("1", 1.0)]
    [TestCase("-2", -2.0)]
    [TestCase("+3", 3.0)]
    [TestCase("0", 0.0)]
    [TestCase(" 42 ", 42.0)]
    [TestCase("9223372036854775807", 9.223372036854776E18)]
    public void ParseInt_Valid(string input, double expected)
    {
        Assert.That(MpvConfOptionHelper.ParseInt(input), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("abc")]
    [TestCase("1.5")]
    [TestCase("1e3")]
    [TestCase("1,000")]
    [TestCase("1.2.3")]
    [TestCase("--2")]
    public void ParseInt_Invalid_IsNaN(string? input)
    {
        Assert.That(double.IsNaN(MpvConfOptionHelper.ParseInt(input)), Is.True);
    }

    [TestCase("1", 1.0)]
    [TestCase("-2.5", -2.5)]
    [TestCase("+0.5", 0.5)]
    [TestCase(" 3.0 ", 3.0)]
    [TestCase("1e3", 1000.0)]
    [TestCase("-1.5e-2", -0.015)]
    public void ParseFloat_Valid(string input, double expected)
    {
        Assert.That(MpvConfOptionHelper.ParseFloat(input), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("abc")]
    [TestCase("1,000")]
    [TestCase("1.2.3")]
    public void ParseFloat_Invalid_IsNaN(string? input)
    {
        Assert.That(double.IsNaN(MpvConfOptionHelper.ParseFloat(input)), Is.True);
    }

    [TestCase(1.0, "1")]
    [TestCase(1.5, "1.5")]
    [TestCase(-0.25, "-0.25")]
    public void FormatNumber(double value, string expected)
    {
        Assert.That(MpvConfOptionHelper.FormatNumber(value), Is.EqualTo(expected));
    }
}
