using System.Text.RegularExpressions;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Services.Metadata;
using ComponentVault.Core.Services.Snippets;
using Microsoft.Extensions.Time.Testing;

namespace ComponentVault.Tests;

/// <summary>
/// Garante que o catálogo e o código real da Lib não se descolem: todo snippet citado
/// existe marcado no fonte e toda variante tem uma View de preview registrada.
/// </summary>
public partial class CatalogConsistencyTests
{
    private static readonly string LibDirectory = Path.Combine(FindRepositoryRoot(), "src", "ComponentVault.Lib");

    private readonly ComponentMetadataService _metadata = new(new FakeTimeProvider());

    [Fact]
    public void Todo_snippet_citado_no_catalogo_existe_no_codigo_da_lib()
    {
        var repository = new SnippetRepository([new LibSourceFiles()]);

        var missing = _metadata.GetAll()
            .SelectMany(i => i.Variants)
            .SelectMany(v => v.Snippets.Select(s => $"{v.Id} -> {s.SnippetId}"))
            .Where(pair => repository.Find(pair.Split(" -> ")[1]) is null)
            .ToArray();

        Assert.True(missing.Length == 0, "Snippets sem marcação no fonte:\n" + string.Join('\n', missing));
    }

    [Fact]
    public void Toda_variante_tem_uma_view_de_preview()
    {
        var registered = Directory.EnumerateFiles(LibDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f))
            .SelectMany(f => PreviewAttributeRegex().Matches(File.ReadAllText(f)).Select(m => m.Groups["id"].Value))
            .ToHashSet(StringComparer.Ordinal);

        var missing = _metadata.GetAll().SelectMany(i => i.Variants).Select(v => v.Id).Where(id => !registered.Contains(id)).ToArray();

        Assert.True(missing.Length == 0, "Variantes sem [ShowcasePreview]:\n" + string.Join('\n', missing));
    }

    [GeneratedRegex("""\[ShowcasePreview\("(?<id>[^"]+)"\)\]""")]
    private static partial Regex PreviewAttributeRegex();

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    // Parte do caminho deste arquivo (fixado na compilação), não da pasta do binário:
    // funciona com --artifacts-path e saídas customizadas de CI.
    private static string FindRepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(sourceFile)!);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ComponentVault.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Raiz do repositório (ComponentVault.slnx) não encontrada.");
    }

    private sealed class LibSourceFiles : ISnippetSource
    {
        public IEnumerable<SnippetFile> GetFiles() =>
            Directory.EnumerateFiles(LibDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => (f.EndsWith(".xaml", StringComparison.Ordinal) || f.EndsWith(".cs", StringComparison.Ordinal)) && !IsBuildOutput(f))
                .Select(f => new SnippetFile(f, File.ReadAllText(f)));
    }
}
