using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Navigation;
using ComponentVault.Core.Presentation;
using ComponentVault.Core.Services.Metadata;

namespace ComponentVault.Core.ViewModels;

public sealed partial class CatalogViewModel : BaseViewModel
{
    private readonly IComponentMetadataService _metadata;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(ResultSummary))]
    private IReadOnlyList<ComponentCardModel> _results = [];

    /// <summary>Resultados agrupados por categoria, na ordem do enum (para a CollectionView agrupada).</summary>
    [ObservableProperty]
    private IReadOnlyList<ComponentGroup> _groups = [];

    public CatalogViewModel(IComponentMetadataService metadata, INavigationService navigation)
    {
        _metadata = metadata;
        _navigation = navigation;
        Title = "Catálogo";

        Filters =
        [
            new OriginFilterOption(null, "Todos") { IsSelected = true },
            .. Enum.GetValues<ComponentOrigin>()
                .Where(o => metadata.GetAll().Any(i => i.Origins.Contains(o)))
                .Select(o => new OriginFilterOption(o, Labels.Origin(o))),
        ];

        ApplyFilter();
    }

    /// <summary>Chips de origem. Só aparecem origens que existem no catálogo.</summary>
    public IReadOnlyList<OriginFilterOption> Filters { get; }

    public ComponentOrigin? SelectedOrigin => Filters.First(f => f.IsSelected).Origin;

    public bool IsEmpty => Results.Count == 0;

    public string ResultSummary => IsEmpty
        ? "Nenhum componente encontrado"
        : Labels.Count(Results.Count, "componente", "componentes");

    // Filtra a cada tecla, sem botão de confirmar (RF04). O catálogo é pequeno e em memória.
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void SelectFilter(OriginFilterOption? option)
    {
        if (option is null || option.IsSelected) return;

        foreach (var filter in Filters) filter.IsSelected = ReferenceEquals(filter, option);
        OnPropertyChanged(nameof(SelectedOrigin));
        ApplyFilter();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectFilter(Filters[0]);
    }

    [RelayCommand]
    private Task OpenComponentAsync(ComponentCardModel? card) =>
        card is null
            ? Task.CompletedTask
            : _navigation.GoToAsync(Routes.ComponentDetail, new Dictionary<string, object> { [Routes.Params.ComponentId] = card.Id });

    private void ApplyFilter()
    {
        Results = _metadata.Search(SearchText, SelectedOrigin)
            .Select(i => new ComponentCardModel(i, _metadata.IsNew(i)))
            .ToArray();

        Groups = Results
            .GroupBy(r => r.Item.Category)
            .OrderBy(g => g.Key)
            .Select(g => new ComponentGroup(Labels.Category(g.Key), g.ToArray()))
            .ToArray();
    }
}

/// <summary>Grupo da lista do catálogo. É uma lista (exigência da CollectionView agrupada) com um título.</summary>
public sealed class ComponentGroup(string title, IReadOnlyList<ComponentCardModel> items) : List<ComponentCardModel>(items)
{
    public string Title { get; } = title;

    public string CountText => Labels.Count(Count, "componente", "componentes");
}

public sealed partial class OriginFilterOption(ComponentOrigin? origin, string label) : ObservableObject
{
    public ComponentOrigin? Origin { get; } = origin;

    public string Label { get; } = label;

    [ObservableProperty]
    private bool _isSelected;
}
