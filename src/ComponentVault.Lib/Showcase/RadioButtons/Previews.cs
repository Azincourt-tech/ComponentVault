using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.RadioButtons;

[ShowcasePreview("radio.base")]
public partial class RadioBasePreview : ContentView
{
    public RadioBasePreview() => InitializeComponent();
}

[ShowcasePreview("radio.styled")]
public partial class RadioStyledPreview : ContentView
{
    public RadioStyledPreview() => InitializeComponent();

    private void OnPlanChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || sender is not RadioButton radio || SelectionLabel is null) return;

        SelectionLabel.Text = radio.Value switch
        {
            "free" => "Plano escolhido: Gratuito",
            "team" => "Plano escolhido: Equipe",
            _ => "Plano escolhido: Pro",
        };
    }
}

[ShowcasePreview("radio.uranium")]
public partial class RadioUraniumPreview : ContentView
{
    public RadioUraniumPreview() => InitializeComponent();
}

[ShowcasePreview("radio.custom")]
public partial class RadioCustomPreview : ContentView
{
    private static readonly string[] Summaries =
    [
        "Chega entre 3 e 5 de dezembro, sem custo.",
        "Chega amanhã até as 18h, por R$ 24,90.",
        "Retire hoje a partir das 16h, sem custo.",
    ];

    public RadioCustomPreview()
    {
        InitializeComponent();
        Select(0);
    }

    private void OnOptionTapped(object? sender, TappedEventArgs e)
    {
        if (int.TryParse(e.Parameter as string, out var index)) Select(index);
    }

    // Grupo de escolha única: marca uma opção e desmarca as outras; o card escolhido ganha destaque.
    private void Select(int index)
    {
        (Border Card, AnimatedRadio Radio)[] options = [(StandardOption, StandardRadio), (ExpressOption, ExpressRadio), (PickupOption, PickupRadio)];
        for (var i = 0; i < options.Length; i++)
        {
            var selected = i == index;
            options[i].Radio.IsSelected = selected;
            options[i].Card.SetThemeBrush(Border.StrokeProperty, selected ? "Accent" : "Hairline");
            options[i].Card.StrokeThickness = selected ? 1.5 : 1;
            options[i].Card.SetThemeColor(VisualElement.BackgroundColorProperty, selected ? "AccentTint" : "Surface");
            SemanticProperties.SetHint(options[i].Card, selected ? "Selecionada" : "Não selecionada");
        }
        SummaryLabel.Text = Summaries[index];
    }
}
