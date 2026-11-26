namespace ComponentVault.Lib.Showcase.Alerts;

[ShowcasePreview("alert.base")]
public partial class AlertBasePreview : ContentView
{
    public AlertBasePreview() => InitializeComponent();
}

[ShowcasePreview("alert.styled")]
public partial class AlertStyledPreview : ContentView
{
    public AlertStyledPreview()
    {
        InitializeComponent();
        foreach (var callout in AlertStack.Children.OfType<Callout>())
        {
            callout.Dismissed += (_, _) => ResetButton.IsVisible = true;
        }
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        foreach (var callout in AlertStack.Children.OfType<Callout>()) callout.IsVisible = true;
        ResetButton.IsVisible = false;
    }
}

[ShowcasePreview("alert.custom")]
public partial class AlertCustomPreview : ContentView
{
    public AlertCustomPreview() => InitializeComponent();

    private void OnRestoreClicked(object? sender, EventArgs e) => Stack.Restore();
}
