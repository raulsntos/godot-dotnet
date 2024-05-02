using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Godot.BindingsGenerator;

internal static class XMLDocsLoader
{
    private static string XMLConstant(string value)
    {
        value = NamingUtils.SnakeToPascalCase(value);
        return value switch
        {
            "@gdscript.nan" => "NaN",
            "@gdscript.tau" => "Tau",
            _ => value,
        };
    }

    private static string ConvertSignal(string param)
    {
        return SourceCodeWriter.EscapeIdentifier(NamingUtils.SnakeToPascalCase(param));
    }

    private static string ConvertEnum(string param)
    {
        return SourceCodeWriter.EscapeIdentifier(NamingUtils.SnakeToPascalCase(param));
    }
    private static string ConvertMethod(string param)
    {
        return SourceCodeWriter.EscapeIdentifier(NamingUtils.SnakeToPascalCase(param));
    }

    private static string ConvertMember(string param)
    {
        return SourceCodeWriter.EscapeIdentifier(NamingUtils.SnakeToPascalCase(param));
    }

    private static string ConvertParam(string param)
    {
        param = NamingUtils.SnakeToPascalCase(param);
        return SourceCodeWriter.EscapeIdentifier(char.ToLower(param[0], CultureInfo.CurrentCulture) + param.Substring(1));
    }

    /// This still has a lot of work but is better than nothing
    private static readonly (string, MatchEvaluator)[] _xmlReplacements = [
        (@"<", x => "&lt;"),
        (@">", x => "&gt;"),
        (@"&", x => "&amp;"),
        (@"\[b\](?<a>.+?)\[/b\]", x => $"<b>{x.Groups["a"].Captures[0].Value}</b>"),
        (@"\[i\](?<a>.+?)\[/i\]", x => $"<i>{x.Groups["a"].Captures[0].Value}</i>"),
        (@"\[constant (?<a>\S+?)\]", x => $"<see cref=\"{XMLConstant(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[code\](?<a>.+?)\[/code\]", x => $"<c>{x.Groups["a"].Captures[0].Value}</c>"),
        (@"\[param (?<a>\S+?)\]",x => $"<paramref name=\"{ConvertParam(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[method (?<a>\S+?)\]", x => $"<see cref=\"{ConvertMethod(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[member (?<a>\S+?)\]", x => $"<see cref=\"{ConvertMember(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[enum (?<a>\S+?)\]",x => $"<see cref=\"{ConvertEnum(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[signal (?<a>\S+?)\]", x => $"<see cref=\"{ConvertSignal(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[url=(?<a>.+?)\](?<b>.+?)\[/url]", x => $"<see href=\"{x.Groups["a"].Captures[0].Value}\">{x.Groups["b"].Captures[0].Value}</see>"),
        (@"\[(?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"),
    ];

    public static void WriteXML(this StringBuilder xmlText, string bbCode)
    {
        var result = string.Empty;
        var lines = bbCode.Trim().Split("\n");
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Contains("[codeblock]"))
            {
                var offset = lines[i].Count(x => x == '\t');
                result += "<code>\n";
                i += 1;
                line = lines[i][offset..];
                while (line.Contains("[/codeblock]") == false)
                {
                    i += 1;
                    result += line + "\n";
                    while (lines[i].Length <= offset) { i += 1; }
                    line = lines[i][offset..];
                }
                result += "</code>\n";
            }
            else if (line.Contains("[codeblocks]"))
            {
                while (line.Contains("[/codeblocks]") == false)
                {
                    i += 1;
                    line = lines[i].Trim();
                    if (line.Contains("[csharp]"))
                    {
                        var offset = lines[i].Count(x => x == '\t');
                        result += "<code>\n";
                        i += 1;
                        line = lines[i][offset..];
                        while (line.Contains("[/csharp]") == false)
                        {
                            i += 1;
                            result += line + "\n";
                            while (lines[i].Length <= offset) { i += 1; }
                            line = lines[i][offset..];
                        }
                        result += "</code>\n";
                    }
                }
            }
            else
            {
                foreach (var (pattern, replacement) in _xmlReplacements)
                {
                    line = Regex.Replace(line, pattern, replacement);
                }
                result += line + "<br/>" + "\n";
            }
        }
        xmlText.Append(result);
    }

    public static void WriteSummary(this StringBuilder xmlText, string? comment, string? shortComment = null)
    {
        xmlText.Append("<summary>\n");
        if (shortComment is not null)
        {
            xmlText.Append("<remarks>\n");
            WriteXML(xmlText, shortComment);
            xmlText.Append("</remarks>\n");
        }
        if (comment is not null)
        {
            WriteXML(xmlText, comment);
        }
        xmlText.Append("</summary>");
    }


}
