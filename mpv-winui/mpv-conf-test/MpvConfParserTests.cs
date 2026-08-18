using mpv_winui.Modules.MpvConf.Conf;

namespace mpv_conf_test;

[TestFixture]
public class MpvConfParserTests
{
    [Test]
    public void ParsesSimpleOption()
    {
        var lines = MpvConfParser.Parse("hwdec=auto\n");
        Assert.That(lines, Has.Count.EqualTo(2));

        var option = lines[0];
        Assert.That(option.Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(option.Key, Is.EqualTo("hwdec"));
        Assert.That(option.Value, Is.EqualTo("auto"));
        Assert.That(option.Enabled, Is.True);
        Assert.That(option.Section, Is.Empty);
        Assert.That(option.LineNumber, Is.EqualTo(0));
    }

    [Test]
    public void ParsesValueWithEquals()
    {
        var option = MpvConfParser.Parse("msg-level=all=error\n")[0];
        Assert.That(option.Key, Is.EqualTo("msg-level"));
        Assert.That(option.Value, Is.EqualTo("all=error"));
    }

    [Test]
    public void ParsesQuotedValueContainingHash()
    {
        var option = MpvConfParser.Parse("user-agent=\"a#b c\"\n")[0];
        Assert.That(option.Value, Is.EqualTo("a#b c"));
    }

    [Test]
    public void ParsesSingleQuotedValue()
    {
        var option = MpvConfParser.Parse("key='value with spaces'\n")[0];
        Assert.That(option.Value, Is.EqualTo("value with spaces"));
    }

    [Test]
    public void ParsesInlineComment_Unquoted()
    {
        var option = MpvConfParser.Parse("volume=50 # default volume\n")[0];
        Assert.That(option.Key, Is.EqualTo("volume"));
        Assert.That(option.Value, Is.EqualTo("50"));
        Assert.That(option.Raw, Does.Contain("# default volume"));
    }

    [Test]
    public void ParsesInlineComment_Quoted()
    {
        var option = MpvConfParser.Parse("osc=no # comment\n")[0];
        Assert.That(option.Value, Is.EqualTo("no"));
    }

    [Test]
    public void ValueTrimmed_CommentPreservedExactly()
    {
        var option = MpvConfParser.Parse("key=val  # hi\n")[0];
        Assert.That(option.Value, Is.EqualTo("val"));
        Assert.That(option.Raw, Is.EqualTo("key=val  # hi"));
    }

    [Test]
    public void ParsesDisabledOption()
    {
        var option = MpvConfParser.Parse("# osc=no\n")[0];
        Assert.That(option.Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(option.Key, Is.EqualTo("osc"));
        Assert.That(option.Value, Is.EqualTo("no"));
        Assert.That(option.Enabled, Is.False);
    }

    [Test]
    public void ParsesDisabledBareOption()
    {
        var option = MpvConfParser.Parse("# fullscreen\n")[0];
        Assert.That(option.Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(option.Key, Is.EqualTo("fullscreen"));
        Assert.That(option.Value, Is.Empty);
        Assert.That(option.Enabled, Is.False);
    }

    [Test]
    public void MultiHashLineIsComment()
    {
        var lines = MpvConfParser.Parse("### Default\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Comment));
        Assert.That(lines[0].Raw, Is.EqualTo("### Default"));
    }

    [Test]
    public void HashOnlyLineIsComment()
    {
        var lines = MpvConfParser.Parse("#######\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Comment));
    }

    [Test]
    public void PlainNoteAfterHashIsComment()
    {
        var lines = MpvConfParser.Parse("# this is a note\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Comment));
    }

    [Test]
    public void BlankLineDetected()
    {
        var lines = MpvConfParser.Parse("\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Blank));
    }

    [Test]
    public void ParsesSectionHeader()
    {
        var lines = MpvConfParser.Parse("[mpvw-hdr]\nhwdec=auto\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Section));
        Assert.That(lines[0].Section, Is.EqualTo("mpvw-hdr"));

        Assert.That(lines[1].Section, Is.EqualTo("mpvw-hdr"));
    }

    [Test]
    public void UnquotedValueWithQuotesKeptAsIs()
    {
        var option = MpvConfParser.Parse("profile-cond=p[\"user-data/mpv/color-kind\"] == \"HDR\"\n")[0];
        Assert.That(option.Value, Is.EqualTo("p[\"user-data/mpv/color-kind\"] == \"HDR\""));
    }

    [Test]
    public void DoubleDashPrefixParsed()
    {
        var option = MpvConfParser.Parse("--volume=50\n")[0];
        Assert.That(option.Key, Is.EqualTo("volume"));
        Assert.That(option.Value, Is.EqualTo("50"));
    }

    [Test]
    public void FixedLengthQuotingParsed()
    {
        var option = MpvConfParser.Parse("key=%5%abcde\n")[0];
        Assert.That(option.Value, Is.EqualTo("abcde"));
    }

    [Test]
    public void UnparsableLineIsInvalidAndPreserved()
    {
        var lines = MpvConfParser.Parse("key=\"a\" extra\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Invalid));
        Assert.That(lines[0].Raw, Is.EqualTo("key=\"a\" extra"));
    }

    [Test]
    public void UnquotedValueTakesEverythingUpToComment()
    {
        var option = MpvConfParser.Parse("key=value extra\n")[0];
        Assert.That(option.Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(option.Value, Is.EqualTo("value extra"));
    }

    [Test]
    public void UnquotedValue_TruncatedAtFirstHash_MatchingMpv()
    {
        var option = MpvConfParser.Parse("key=C:\\a#b\n")[0];
        Assert.That(option.Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(option.Value, Is.EqualTo("C:\\a"));
        Assert.That(option.Raw, Is.EqualTo("key=C:\\a#b"));
    }

    [Test]
    public void UnquotedValue_HashAfterQuoteChar_Truncated()
    {
        var option = MpvConfParser.Parse("key=a\"b#c\n")[0];
        Assert.That(option.Value, Is.EqualTo("a\"b"));
        Assert.That(option.Raw, Is.EqualTo("key=a\"b#c"));
    }

    [Test]
    public void UnquotedValue_HashAtValueStart_IsEmptyValueWithComment()
    {
        var option = MpvConfParser.Parse("key=#hidden\n")[0];
        Assert.That(option.Value, Is.EqualTo(""));
        Assert.That(option.Raw, Does.Contain("#hidden"));
    }

    [Test]
    public void UnterminatedQuoteIsInvalid()
    {
        var lines = MpvConfParser.Parse("key=\"unterminated\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public void ValueSetQuotesWhenNeeded()
    {
        var option = MpvConfParser.Parse("key=plain\n")[0];
        option.Value = "a#b";
        Assert.That(option.Raw, Is.EqualTo("key=\"a#b\""));
    }

    [Test]
    public void ValueSetKeepsOriginalQuoteStyle()
    {
        var option = MpvConfParser.Parse("key=\"old\"\n")[0];
        option.Value = "new";
        Assert.That(option.Raw, Is.EqualTo("key=\"new\""));
    }

    [Test]
    public void ToggleDisabledRegeneratesPrefix()
    {
        var option = MpvConfParser.Parse("osc=no\n")[0];
        option.Enabled = false;
        Assert.That(option.Raw, Is.EqualTo("# osc=no"));

        option.Enabled = true;
        Assert.That(option.Raw, Is.EqualTo("osc=no"));
    }

    [Test]
    public void SkipsBom()
    {
        var lines = MpvConfParser.Parse("\uFEFFhwdec=auto\n");
        Assert.That(lines[0].Key, Is.EqualTo("hwdec"));
    }

    [Test]
    public void MultipleOptionsSameKeyAreAllKept()
    {
        var lines = MpvConfParser.Parse("watch-later-options-remove=sub-pos\nwatch-later-options-remove=osd-margin-y\n");
        var options = lines.Where(l => l.IsOption).ToList();
        Assert.That(options, Has.Count.EqualTo(2));
        Assert.That(options[0].Value, Is.EqualTo("sub-pos"));
        Assert.That(options[1].Value, Is.EqualTo("osd-margin-y"));
    }

    [Test]
    public void ParseArray_Empty_ReturnsNoLines()
    {
        var lines = MpvConfParser.Parse(Array.Empty<string>());
        Assert.That(lines, Is.Empty);
    }

    [Test]
    public void ParseArray_OptionAndSection_PropagatesSection()
    {
        var lines = MpvConfParser.Parse(new[] { "[mpvw-hdr]", "hwdec=auto" });

        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Section));
        Assert.That(lines[0].Section, Is.EqualTo("mpvw-hdr"));
        Assert.That(lines[1].Key, Is.EqualTo("hwdec"));
        Assert.That(lines[1].Section, Is.EqualTo("mpvw-hdr"));
    }

    [Test]
    public void ParseArray_HasNoPhantomTrailingLine()
    {
        var lines = MpvConfParser.Parse(new[] { "hwdec=auto" });
        Assert.That(lines, Has.Count.EqualTo(1));
        Assert.That(lines[0].Key, Is.EqualTo("hwdec"));
    }

    [Test]
    public void ParseArray_DisabledAndBlankLinesKept()
    {
        var lines = MpvConfParser.Parse(new[] { "# osc=no", "", "a=1" });

        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(lines[0].Enabled, Is.False);
        Assert.That(lines[1].Type, Is.EqualTo(MpvConfLineType.Blank));
        Assert.That(lines[2].Value, Is.EqualTo("1"));
    }

    [Test]
    public void ParseArray_KeepsRawCarriageReturn_AndParsesValue()
    {
        var lines = MpvConfParser.Parse(new[] { "a=1\r", "b=2\r" });

        Assert.That(lines[0].Value, Is.EqualTo("1"));
        Assert.That(lines[0].Raw, Is.EqualTo("a=1\r"));
        Assert.That(lines[1].Value, Is.EqualTo("2"));
    }

    [Test]
    public void ParsesSampleConf()
    {
        string sample =
            "#######\n" +
            "### Default\n" +
            "# gpu-shader-cache-dir=~~/cache/shaders_cache\n" +
            "# osc=no\n" +
            "msg-level=all=error\n" +
            "log-file=~~/mpv.log\n" +
            "\n" +
            "[mpvw-hdr]\n" +
            "profile-cond=p[\"user-data/mpv/color-kind\"] == \"HDR\"\n" +
            "d3d11-output-format=rgb10_a2\n" +
            "osd-playing-msg=\"${!playlist-count==1:${playlist-pos-1}/${playlist-count}}\"\n" +
            "screenshot-template=\"~~/screenshots/%F-(%P)-%n\"\n";

        var lines = MpvConfParser.Parse(sample);

        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Comment));
        Assert.That(lines[2].Type, Is.EqualTo(MpvConfLineType.Option));
        Assert.That(lines[2].Enabled, Is.False);
        Assert.That(lines[2].Key, Is.EqualTo("gpu-shader-cache-dir"));

        Assert.That(lines[4].Value, Is.EqualTo("all=error"));

        var section = lines[7];
        Assert.That(section.Type, Is.EqualTo(MpvConfLineType.Section));
        Assert.That(section.Section, Is.EqualTo("mpvw-hdr"));

        Assert.That(lines[8].Section, Is.EqualTo("mpvw-hdr"));
        Assert.That(lines[8].Value, Is.EqualTo("p[\"user-data/mpv/color-kind\"] == \"HDR\""));

        Assert.That(lines[10].Value, Is.EqualTo("${!playlist-count==1:${playlist-pos-1}/${playlist-count}}"));
        Assert.That(lines[11].Value, Is.EqualTo("~~/screenshots/%F-(%P)-%n"));
    }

    [Test]
    public void FixedLengthQuoting_NonAscii_CountsUtf8Bytes()
    {
        var option = MpvConfParser.Parse("key=%6%中文\n")[0];
        Assert.That(option.Value, Is.EqualTo("中文"));
    }

    [Test]
    public void FixedLengthQuoting_CutsMultibyteChar_IsInvalid()
    {
        var lines = MpvConfParser.Parse("key=%1%中文\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public void FixedLengthQuoting_ExceedsRemainingBytes_IsInvalid()
    {
        var lines = MpvConfParser.Parse("key=%5%ab\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public void FixedLengthQuoting_NegativeLength_IsInvalid_NotCrash()
    {
        var lines = MpvConfParser.Parse("key=%-1%ab\n");
        Assert.That(lines[0].Type, Is.EqualTo(MpvConfLineType.Invalid));
    }

    [Test]
    public void ValueSetWithOriginalQuoteConflict_UsesAlternateQuote()
    {
        var option = MpvConfParser.Parse("key=\"old\"\n")[0];
        option.Value = "a\"b";
        Assert.That(option.Raw, Is.EqualTo("key='a\"b'"));
    }

    [Test]
    public void ValueSetWithSingleQuoteConflict_UsesDoubleQuote()
    {
        var option = MpvConfParser.Parse("key='old'\n")[0];
        option.Value = "don't";
        Assert.That(option.Raw, Is.EqualTo("key=\"don't\""));
    }

    [Test]
    public void ValueSetWithBothQuotes_UsesFixedLength()
    {
        var option = MpvConfParser.Parse("key=plain\n")[0];
        option.Value = "a\"b'c#x";
        Assert.That(option.Raw, Is.EqualTo("key=%7%a\"b'c#x"));
    }

    [Test]
    public void ValueSetFixedLength_NonAscii_CountsUtf8Bytes()
    {
        var option = MpvConfParser.Parse("key=plain\n")[0];
        option.Value = "'中\"";
        Assert.That(option.Raw, Is.EqualTo("key=%5%'中\""));
    }

    [Test]
    public void ValueSetKeepsOriginalQuoteStyle_WhenValueAbsentOfQuote()
    {
        var option = MpvConfParser.Parse("key='old'\n")[0];
        option.Value = "new";
        Assert.That(option.Raw, Is.EqualTo("key='new'"));
    }

    [TestCase("hwdec", true)]
    [TestCase("video-zoom", true)]
    [TestCase("video_zoom", true)]
    [TestCase("_x", true)]
    [TestCase("x1", true)]
    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("-x", false)]
    [TestCase("--x", false)]
    [TestCase("x y", false)]
    [TestCase("x.y", false)]
    [TestCase("x#y", false)]
    public void IsValidOptionKey(string? key, bool expected)
    {
        Assert.That(MpvConfParser.IsValidOptionKey(key), Is.EqualTo(expected));
    }
}
