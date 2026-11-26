using System.Globalization;
using MauiIcons.Fluent;

namespace ComponentVault.Lib.Showcase.SearchBars;

[ShowcasePreview("searchbar.base")]
public partial class SearchBarBasePreview : ContentView
{
    public SearchBarBasePreview() => InitializeComponent();

    private void OnSearch(object? sender, EventArgs e) =>
        ResultLabel.Text = string.IsNullOrWhiteSpace(Search.Text) ? "Digite algo para buscar" : $"Buscou por “{Search.Text}”";
}

/// <summary>Resultado com o trecho encontrado separado, para destacá-lo em negrito.</summary>
public sealed record SearchResult(FluentIcons Icon, string Before, string Match, string After);

[ShowcasePreview("searchbar.styled")]
public partial class SearchBarStyledPreview : ContentView
{
    private static readonly string[] Catalog =
    [
        "Button", "Entry", "Editor", "SearchBar", "Stepper", "Picker", "DatePicker", "Switch", "CheckBox",
        "RadioButton", "Slider", "ProgressBar", "CarouselView", "CollectionView", "Border", "Chips",
    ];

    private static readonly string[] Recent = ["Picker", "Slider", "Chips"];

    private string _query = string.Empty;

    public SearchBarStyledPreview()
    {
        InitializeComponent();
        Refresh();
        BindingContext = this;
    }

    public string Query
    {
        get => _query;
        set
        {
            if (_query == value) return;
            _query = value ?? string.Empty;
            OnPropertyChanged();
            Refresh();
        }
    }

    public IReadOnlyList<SearchResult> Results { get; private set; } = [];

    public string ListTitle { get; private set; } = string.Empty;

    public bool HasResults => Results.Count > 0;

    public bool IsEmpty => !HasResults && Query.Trim().Length > 0;

    public string EmptyText => $"Nada encontrado para “{Query.Trim()}”. Tente o nome do controle, como Picker ou Slider.";

    private void Refresh()
    {
        var term = Query.Trim();
        if (term.Length == 0)
        {
            Results = Recent.Select(r => new SearchResult(FluentIcons.Clock20, r, string.Empty, string.Empty)).ToArray();
            ListTitle = "Buscas recentes";
        }
        else
        {
            var compare = CultureInfo.InvariantCulture.CompareInfo;
            Results = Catalog
                .Select(name => (name, index: compare.IndexOf(name, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace)))
                .Where(x => x.index >= 0)
                .OrderBy(x => x.index)
                .Select(x => new SearchResult(
                    FluentIcons.Search20,
                    x.name[..x.index],
                    x.name.Substring(x.index, term.Length),
                    x.name[(x.index + term.Length)..]))
                .ToArray();
            ListTitle = Results.Count == 1 ? "1 resultado" : $"{Results.Count} resultados";
        }

        OnPropertyChanged(nameof(Results));
        OnPropertyChanged(nameof(ListTitle));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EmptyText));
    }
}

[ShowcasePreview("searchbar.uranium")]
public partial class SearchBarUraniumPreview : ContentView
{
    // Sugestões: os nomes do próprio catálogo (num app real, viriam do serviço de busca).
    private static readonly string[] Components =
    [
        "Alert", "Avatar", "Badge", "Button", "Card", "Carousel", "Chart", "CheckBox", "Chips",
        "Código OTP", "DatePicker", "Editor", "Empty state", "Entry", "Glass", "Ícones", "List",
        "Picker", "Popup", "Progress", "RadioButton", "Rating", "SearchBar", "Segmented control",
        "Skeleton", "Slider", "Stepper", "Switch", "Tabs", "Toast",
    ];

    public SearchBarUraniumPreview()
    {
        InitializeComponent();
        SearchField.ItemsSource = Components;
        SearchField.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(SearchField.SelectedText) || string.IsNullOrEmpty(SearchField.SelectedText)) return;
            ResultLabel.Text = $"Abrindo {SearchField.SelectedText}.";
        };
    }
}

[ShowcasePreview("searchbar.custom")]
public partial class SearchBarCustomPreview : ContentView
{
    public SearchBarCustomPreview()
    {
        InitializeComponent();
        // Os primeiros itens viram as sugestões de quando o campo está vazio.
        Search.Items =
        [
            "Picker", "Slider", "Chips", "Alert", "Avatar", "Badge", "Button", "Card", "Carousel", "Chart",
            "CheckBox", "Código OTP", "DatePicker", "Editor", "Empty state", "Entry", "Glass", "Ícones", "List",
            "Popup", "Progress", "RadioButton", "Rating", "SearchBar", "Segmented control", "Skeleton",
            "Stepper", "Switch", "Tabs", "Toast",
        ];
    }

    private void OnItemChosen(object? sender, string item) => ResultLabel.Text = $"Abrindo {item}.";
}
