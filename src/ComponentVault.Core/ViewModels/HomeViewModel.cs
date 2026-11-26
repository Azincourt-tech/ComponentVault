using CommunityToolkit.Mvvm.Input;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Navigation;
using ComponentVault.Core.Presentation;
using ComponentVault.Core.Services.Metadata;

namespace ComponentVault.Core.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    public const int RecentCount = 5;

    private readonly INavigationService _navigation;

    public HomeViewModel(IComponentMetadataService metadata, INavigationService navigation)
    {
        _navigation = navigation;
        Title = "ComponentVault";

        var all = metadata.GetAll();
        Recent = metadata.GetRecent(RecentCount).Select(i => new ComponentCardModel(i, metadata.IsNew(i))).ToArray();

        var variants = all.SelectMany(i => i.Variants).ToArray();
        var libraries = variants.Where(v => v.PackageId is not null).Select(v => v.PackageId).Distinct().Count();
        Stats =
        [
            new StatModel(all.Count, Labels.Plural(all.Count, "componente", "componentes")),
            new StatModel(variants.Length, Labels.Plural(variants.Length, "variante", "variantes")),
            new StatModel(libraries, Labels.Plural(libraries, "biblioteca", "bibliotecas")),
        ];
    }

    public IReadOnlyList<ComponentCardModel> Recent { get; }

    public IReadOnlyList<StatModel> Stats { get; }

    /// <summary>Os níveis da vitrine, explicados na Home para quem chega pela primeira vez.</summary>
    public IReadOnlyList<TierModel> Tiers { get; } =
    [
        new(VariantTier.Base, ComponentOrigin.Native, Labels.Tier(VariantTier.Base), "O controle como o MAUI entrega, sem estilo."),
        new(VariantTier.Styled, ComponentOrigin.Native, Labels.Tier(VariantTier.Styled), "O mesmo controle com os tokens do design system."),
        new(VariantTier.Custom, ComponentOrigin.Custom, Labels.Tier(VariantTier.Custom), "Desenhado do zero com Graphics ou SkiaSharp."),
        new(VariantTier.Library, ComponentOrigin.FreeLibrary, Labels.Tier(VariantTier.Library), "Construído com UraniumUI, Toolkit, LiveCharts e outras."),
    ];

    [RelayCommand]
    private Task OpenComponentAsync(ComponentCardModel? card) =>
        card is null
            ? Task.CompletedTask
            : _navigation.GoToAsync(Routes.ComponentDetail, new Dictionary<string, object> { [Routes.Params.ComponentId] = card.Id });

    [RelayCommand]
    private Task OpenCatalogAsync() => _navigation.GoToAsync(Routes.Catalog);
}

public sealed record StatModel(int Value, string Label);

public sealed record TierModel(VariantTier Tier, ComponentOrigin Origin, string Title, string Description);
