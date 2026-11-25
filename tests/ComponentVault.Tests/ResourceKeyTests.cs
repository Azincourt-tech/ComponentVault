using System.Text.RegularExpressions;

namespace ComponentVault.Tests;

/// <summary>
/// Chaves de recurso inexistentes só estouram em tempo de execução ({toolkit:AppThemeResource}
/// e ThemeColors resolvem na hora), derrubando a tela. Este teste pega isso antes.
/// </summary>
public partial class ResourceKeyTests
{
    // Chaves montadas em código (ex.: $"Origin{Kind}") não são literais e ficam de fora.
    [Fact]
    public void Toda_chave_de_recurso_usada_esta_definida()
    {
        var root = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Where(f => (f.EndsWith(".xaml", StringComparison.Ordinal) || f.EndsWith(".cs", StringComparison.Ordinal))
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                        && !f.Contains($"{Path.DirectorySeparatorChar}Platforms{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(f => (Path: f, Text: XmlCommentRegex().Replace(File.ReadAllText(f), string.Empty)))
            .ToArray();

        var defined = files
            .Where(f => f.Path.EndsWith(".xaml", StringComparison.Ordinal))
            .SelectMany(f => DefinedKeyRegex().Matches(f.Text).Select(m => m.Groups["key"].Value))
            .ToHashSet(StringComparer.Ordinal);

        var missing = files
            .SelectMany(f => UsedKeyRegex().Matches(f.Text)
                .Where(m => !m.Groups["dynamic"].Success)
                .Select(m => m.Groups["key"].Value)
                .Where(k => !defined.Contains(k))
                .Select(k => $"{Path.GetRelativePath(root, f.Path)}: {k}"))
            .Distinct()
            .ToArray();

        Assert.True(missing.Length == 0, "Chaves de recurso não definidas:\n" + string.Join('\n', missing));
    }

    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex XmlCommentRegex();

    [GeneratedRegex("x:Key=\"(?<key>[^\"]+)\"")]
    private static partial Regex DefinedKeyRegex();

    [Fact]
    public void As_tres_paletas_definem_as_mesmas_chaves()
    {
        var themeDir = Path.Combine(FindRepositoryRoot(), "src", "ComponentVault.Lib", "Resources", "Theme");
        var palettes = new[] { "LightPalette.xaml", "DarkPalette.xaml", "GlassPalette.xaml" }
            .ToDictionary(
                name => name,
                name => DefinedKeyRegex().Matches(File.ReadAllText(Path.Combine(themeDir, name)))
                    .Select(m => m.Groups["key"].Value)
                    .ToHashSet(StringComparer.Ordinal));

        var all = palettes.Values.SelectMany(k => k).ToHashSet(StringComparer.Ordinal);
        var gaps = palettes
            .SelectMany(p => all.Except(p.Value).Select(missing => $"{p.Key} não define {missing}"))
            .ToArray();

        Assert.True(gaps.Length == 0, string.Join('\n', gaps));
    }

    [GeneratedRegex("""(?:\{StaticResource\s+|\{DynamicResource\s+|\{toolkit:AppThemeResource\s+|SetDynamicResource\([^,]+,\s*"|SetThemeColor\([^,]+,\s*"|SetThemeBrush\([^,]+,\s*"|Resolve\(")(?<key>\w+)(?<dynamic>\{)?""")]
    private static partial Regex UsedKeyRegex();

    private static string FindRepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(sourceFile)!);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ComponentVault.slnx"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("Raiz do repositório não encontrada.");
    }
}
