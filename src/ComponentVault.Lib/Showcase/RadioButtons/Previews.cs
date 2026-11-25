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
