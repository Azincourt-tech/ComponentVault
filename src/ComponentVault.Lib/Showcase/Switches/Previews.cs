namespace ComponentVault.Lib.Showcase.Switches;

[ShowcasePreview("switch.base")]
public partial class SwitchBasePreview : ContentView
{
    public SwitchBasePreview() => InitializeComponent();
}

[ShowcasePreview("switch.styled")]
public partial class SwitchStyledPreview : ContentView
{
    public SwitchStyledPreview() => InitializeComponent();

    // A linha inteira alterna o switch: alvo de toque maior que o controle.
    private void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Switch { IsEnabled: true } target) target.IsToggled = !target.IsToggled;
    }
}

[ShowcasePreview("switch.custom")]
public partial class SwitchCustomPreview : ContentView
{
    public SwitchCustomPreview() => InitializeComponent();
}
