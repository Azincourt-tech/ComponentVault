using System.Globalization;
using System.Text;
using ComponentVault.Core.Models;
using ComponentVault.Core.Presentation;

namespace ComponentVault.Core.Services.Metadata;

public interface IComponentMetadataService
{
    IReadOnlyList<ComponentItem> GetAll();

    ComponentItem? GetById(string id);

    /// <summary>
    /// Filtra por texto (nome, tags, resumo, título e pacote das variantes) e, opcionalmente,
    /// pelas origens: o componente entra se tiver pelo menos uma variante com a origem pedida.
    /// </summary>
    IReadOnlyList<ComponentItem> Search(string? query, ComponentOrigin? origin = null);

    IReadOnlyList<ComponentItem> GetRecent(int count);

    bool IsNew(ComponentItem item);
}

/// <summary>
/// Fonte única do catálogo (RF06). Adicionar um componente = criar as Views de preview
/// e incluir uma entrada em <see cref="ComponentCatalog"/>.
/// </summary>
public sealed class ComponentMetadataService : IComponentMetadataService
{
    /// <summary>Por quantos dias um componente recém-adicionado exibe o badge "Novo".</summary>
    public const int NewBadgeDays = 30;

    private readonly IReadOnlyList<ComponentItem> _items;
    private readonly Dictionary<string, ComponentItem> _byId;
    private readonly TimeProvider _time;

    public ComponentMetadataService(TimeProvider time)
        : this(ComponentCatalog.Items, time)
    {
    }

    internal ComponentMetadataService(IReadOnlyList<ComponentItem> items, TimeProvider time)
    {
        Validate(items);
        _items = items.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        _byId = _items.ToDictionary(i => i.Id, StringComparer.OrdinalIgnoreCase);
        _time = time;
    }

    public IReadOnlyList<ComponentItem> GetAll() => _items;

    public ComponentItem? GetById(string id) => _byId.GetValueOrDefault(id);

    public IReadOnlyList<ComponentItem> Search(string? query, ComponentOrigin? origin = null)
    {
        var terms = Normalize(query).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _items
            .Where(i => origin is null || i.Variants.Any(v => v.Origin == origin))
            .Where(i => terms.Length == 0 || terms.All(t => SearchText(i).Contains(t, StringComparison.Ordinal)))
            .ToArray();
    }

    public IReadOnlyList<ComponentItem> GetRecent(int count) =>
        _items.OrderByDescending(i => i.AddedOn).ThenBy(i => i.Name).Take(count).ToArray();

    public bool IsNew(ComponentItem item)
    {
        var today = DateOnly.FromDateTime(_time.GetLocalNow().Date);
        return item.AddedOn > today.AddDays(-NewBadgeDays);
    }

    // Cache do texto pesquisável: o catálogo é imutável, então calcula uma vez por item.
    private readonly Dictionary<string, string> _searchText = new(StringComparer.Ordinal);

    private string SearchText(ComponentItem item)
    {
        lock (_searchText)
        {
            if (_searchText.TryGetValue(item.Id, out var cached)) return cached;

            var parts = new List<string> { item.Name, item.Summary, Labels.Category(item.Category) };
            parts.AddRange(item.Tags);
            foreach (var v in item.Variants)
            {
                parts.Add(v.Title);
                if (v.PackageId is not null) parts.Add(v.PackageId);
            }

            var text = Normalize(string.Join(' ', parts));
            _searchText[item.Id] = text;
            return text;
        }
    }

    /// <summary>Minúsculas e sem acento, para "botao" achar "Botão".</summary>
    internal static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static void Validate(IReadOnlyList<ComponentItem> items)
    {
        var duplicatedComponent = items.GroupBy(i => i.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicatedComponent is not null)
            throw new InvalidOperationException($"Id de componente duplicado: '{duplicatedComponent.Key}'.");

        var duplicatedVariant = items.SelectMany(i => i.Variants)
            .GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicatedVariant is not null)
            throw new InvalidOperationException($"Id de variante duplicado: '{duplicatedVariant.Key}'.");

        var empty = items.FirstOrDefault(i => i.Variants.Count == 0);
        if (empty is not null)
            throw new InvalidOperationException($"O componente '{empty.Id}' não tem nenhuma variante.");
    }
}
