using System;
using System.Collections.Generic;
using System.Text;

namespace mpv_winui.Modules.MpvConf.Conf;

public static class MpvConfParser
{
    public static List<MpvConfLine> Parse(string text)
    {
        var result = new List<MpvConfLine>();

        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        if (text.StartsWith("\uFEFF", StringComparison.Ordinal))
        {
            text = text[1..];
        }

        string section = string.Empty;
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string raw = lines[i];
            MpvConfLine line = ParseLine(raw, section);
            result.Add(line);

            if (line.Type == MpvConfLineType.Section)
            {
                section = line.Section;
            }
        }

        return result;
    }

    public static List<MpvConfLine> Parse(string[] lines)
    {
        var result = new List<MpvConfLine>();

        if (lines.Length == 0)
        {
            return result;
        }

        string section = string.Empty;

        foreach (var raw in lines)
        {
            MpvConfLine line = ParseLine(raw, section);
            result.Add(line);

            if (line.Type == MpvConfLineType.Section)
            {
                section = line.Section;
            }
        }

        return result;
    }

    private static MpvConfLine ParseLine(string raw, string section)
    {
        string content = raw.TrimEnd('\r');
        string trimmed = content.TrimStart();

        if (trimmed.Length == 0)
        {
            return MpvConfLine.Blank(raw, section);
        }

        if (trimmed[0] == '#')
        {
            string rest = trimmed[1..].TrimStart();
            if (TryParseOption(rest, out string key, out string value, out char? quote, out string inlineComment, out _))
            {
                return MpvConfLine.Option(raw, section, key, value, enabled: false, quote, inlineComment);
            }

            return MpvConfLine.Comment(raw, section);
        }

        if (trimmed[0] == '[')
        {
            int close = trimmed.IndexOf(']');
            if (close > 1)
            {
                string name = trimmed[1..close];
                if (trimmed[(close + 1)..].Trim().Length == 0)
                {
                    return MpvConfLine.SectionLine(raw, name);
                }
            }

            return MpvConfLine.Invalid(raw, section);
        }

        if (TryParseOption(trimmed, out string optionKey, out string optionValue, out char? optionQuote, out string optionInline, out _))
        {
            return MpvConfLine.Option(raw, section, optionKey, optionValue, enabled: true, optionQuote, optionInline);
        }

        return MpvConfLine.Invalid(raw, section);
    }

    public static bool IsValidOptionKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (!IsAsciiAlnum(key[0]) && key[0] != '_')
        {
            return false;
        }

        for (int i = 1; i < key.Length; i++)
        {
            if (!IsAsciiAlnum(key[i]) && key[i] != '_' && key[i] != '-')
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryParseOption(string text, out string key, out string value, out char? quote, out string inlineComment, out bool hasEquals)
    {
        key = string.Empty;
        value = string.Empty;
        quote = null;
        inlineComment = string.Empty;
        hasEquals = false;

        int pos = 0;

        if (text.Length >= 2 && text[0] == '-' && text[1] == '-')
        {
            pos += 2;
        }

        int nameStart = pos;
        while (pos < text.Length && (IsAsciiAlnum(text[pos]) || text[pos] == '_' || text[pos] == '-'))
        {
            pos++;
        }

        if (pos == nameStart)
        {
            return false;
        }

        key = text[nameStart..pos];

        while (pos < text.Length && char.IsWhiteSpace(text[pos]))
        {
            pos++;
        }

        if (pos < text.Length && text[pos] == '=')
        {
            hasEquals = true;
            pos++;

            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
            {
                pos++;
            }

            if (pos < text.Length && (text[pos] == '"' || text[pos] == '\''))
            {
                char q = text[pos];
                quote = q;
                pos++;

                int close = text.IndexOf(q, pos);
                if (close < 0)
                {
                    return false;
                }

                value = text[pos..close];
                pos = close + 1;
            }
            else if (pos < text.Length && text[pos] == '%')
            {
                int digitsStart = pos + 1;
                int p = digitsStart;
                while (p < text.Length && char.IsDigit(text[p]))
                {
                    p++;
                }

                if (p == digitsStart || p >= text.Length || text[p] != '%')
                {
                    return false;
                }

                if (!long.TryParse(text[digitsStart..p], out long length) || length < 0 || length > int.MaxValue)
                {
                    return false;
                }

                byte[] bytes = Encoding.UTF8.GetBytes(text[(p + 1)..]);
                if (length > bytes.Length)
                {
                    return false;
                }

                if (length < bytes.Length && (bytes[(int)length] & 0xC0) == 0x80)
                {
                    return false;
                }

                value = Encoding.UTF8.GetString(bytes, 0, (int)length);
                pos = p + 1 + Encoding.UTF8.GetCharCount(bytes, 0, (int)length);
            }
            else
            {
                int valueStart = pos;
                int end = text.Length;
                int hash = text.IndexOf('#', valueStart);
                if (hash >= 0)
                {
                    end = hash;
                }

                string rawValue = text[valueStart..end];
                value = rawValue.TrimEnd();
                pos = valueStart + value.Length;
            }
        }

        int rest = pos;
        while (rest < text.Length && char.IsWhiteSpace(text[rest]))
        {
            rest++;
        }

        if (rest < text.Length && text[rest] == '#')
        {
            inlineComment = text[pos..];
            return true;
        }

        return rest == text.Length;
    }

    private static bool IsAsciiAlnum(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
}
