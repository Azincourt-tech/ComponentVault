using ComponentVault.Core.Models;
using ComponentVault.Core.Presentation;

namespace ComponentVault.Core.ViewModels;

/// <summary>Projeção de um componente para cards e linhas de lista (Home e Catálogo).</summary>
public sealed class ComponentCardModel(ComponentItem item, bool isNew)
{
    public ComponentItem Item { get; } = item;

    public string Id => Item.Id;

    public string Name => Item.Name;

    public string? Icon => Item.Icon;

    public string Summary => Item.Summary;

    public bool IsNew { get; } = isNew;

    public IReadOnlyList<ComponentOrigin> Origins => Item.Origins;

    /// <summary>Origem usada na cor da miniatura: a mais "rica" (custom, lib) vence a nativa.</summary>
    public ComponentOrigin AccentOrigin => Item.Origins.Count == 0 ? ComponentOrigin.Native : Item.Origins[^1];

    public string CategoryText => Labels.Category(Item.Category);

    public string VariantCountText => Labels.Count(Item.Variants.Count, "nível", "níveis");

    /// <summary>Ex.: "Base MAUI · Estilizado · Biblioteca".</summary>
    public string TiersText => string.Join(" · ", Item.Variants.Select(v => Labels.Tier(v.Tier)).Distinct());

    public string TagsText => string.Join(" · ", Item.Tags.Take(3));
}
