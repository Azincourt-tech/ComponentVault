namespace ComponentVault.Lib.Showcase.EmptyStates;

[ShowcasePreview("emptystate.base")]
public partial class EmptyStateBasePreview : ContentView
{
    public EmptyStateBasePreview() => InitializeComponent();

    /// <summary>Lista sem itens: o que faz o EmptyView aparecer.</summary>
    public static IReadOnlyList<string> NoItems { get; } = [];
}

[ShowcasePreview("emptystate.styled")]
public partial class EmptyStateStyledPreview : ContentView
{
    public EmptyStateStyledPreview() => InitializeComponent();
}

[ShowcasePreview("emptystate.custom")]
public partial class EmptyStateCustomPreview : ContentView
{
    private const string OriginalQuery = "gráfico de radar";
    private string _query = OriginalQuery;

    public EmptyStateCustomPreview()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // O título repete o que a pessoa buscou: ela confere a grafia sem voltar ao campo.
    public string Title => $"Nenhum componente para “{_query}”";

    public IReadOnlyList<string> Suggestions { get; } = ["chart", "gráfico", "LiveCharts", "progresso"];

    // Na demonstração a sugestão só troca o termo; num app real, refaz a busca com ele.
    private void OnSuggestionTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not string suggestion) return;
        _query = suggestion;
        OnPropertyChanged(nameof(Title));
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _query = OriginalQuery;
        OnPropertyChanged(nameof(Title));
    }
}
