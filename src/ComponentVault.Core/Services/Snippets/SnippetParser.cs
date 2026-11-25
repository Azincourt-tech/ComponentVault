using System.Text;
using System.Text.RegularExpressions;

namespace ComponentVault.Core.Services.Snippets;

/// <summary>
/// Extrai regiões marcadas de arquivos de código. Uma região começa numa linha
/// <c>&lt;!-- snippet:id --&gt;</c> (XAML) ou <c>// snippet:id</c> (C#) e termina em
/// <c>&lt;!-- /snippet:id --&gt;</c> ou <c>// /snippet:id</c>.
/// </summary>
/// <remarks>
/// Regiões podem se sobrepor ou aninhar; linhas de marcação nunca entram no texto extraído.
/// A indentação comum é removida para o snippet ficar pronto para colar.
/// </remarks>
public static partial class SnippetParser
{
    [GeneratedRegex(@"^\s*(?:<!--|//)\s*(?<close>/)?snippet:(?<id>[\w.\-]+)\s*(?:-->)?\s*$")]
    private static partial Regex MarkerRegex();

    public static IReadOnlyDictionary<string, string> Parse(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var open = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            var match = MarkerRegex().Match(line);
            if (match.Success)
            {
                var id = match.Groups["id"].Value;
                if (match.Groups["close"].Success)
                {
                    if (open.Remove(id, out var body))
                    {
                        result[id] = Dedent(body);
                    }
                }
                else
                {
                    open[id] = [];
                }

                continue;
            }

            foreach (var body in open.Values)
            {
                body.Add(line);
            }
        }

        return result;
    }

    internal static string Dedent(IReadOnlyList<string> lines)
    {
        var first = 0;
        var last = lines.Count - 1;
        while (first <= last && string.IsNullOrWhiteSpace(lines[first])) first++;
        while (last >= first && string.IsNullOrWhiteSpace(lines[last])) last--;
        if (first > last) return string.Empty;

        var indent = int.MaxValue;
        for (var i = first; i <= last; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            indent = Math.Min(indent, line.Length - line.TrimStart().Length);
        }

        var sb = new StringBuilder();
        for (var i = first; i <= last; i++)
        {
            var line = lines[i];
            sb.Append(line.Length >= indent ? line[indent..].TrimEnd() : line.TrimEnd());
            if (i < last) sb.Append('\n');
        }

        return sb.ToString();
    }
}
