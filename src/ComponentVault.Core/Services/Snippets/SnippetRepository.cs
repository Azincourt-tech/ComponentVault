using ComponentVault.Core.Abstractions;

namespace ComponentVault.Core.Services.Snippets;

public interface ISnippetRepository
{
    /// <summary>Texto do snippet, ou nulo quando nenhum arquivo marca esse id.</summary>
    string? Find(string snippetId);

    IReadOnlyCollection<string> Ids { get; }
}

public sealed class SnippetRepository(IEnumerable<ISnippetSource> sources) : ISnippetRepository
{
    private readonly Lazy<IReadOnlyDictionary<string, string>> _index = new(() => Build(sources));

    public IReadOnlyCollection<string> Ids => (IReadOnlyCollection<string>)_index.Value.Keys;

    public string? Find(string snippetId) =>
        _index.Value.TryGetValue(snippetId, out var text) ? text : null;

    private static IReadOnlyDictionary<string, string> Build(IEnumerable<ISnippetSource> sources)
    {
        var index = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in sources.SelectMany(s => s.GetFiles()))
        {
            foreach (var (id, text) in SnippetParser.Parse(file.Content))
            {
                if (!index.TryAdd(id, text))
                {
                    throw new InvalidOperationException(
                        $"O snippet '{id}' está marcado em mais de um arquivo (último: {file.Path}).");
                }
            }
        }

        return index;
    }
}
