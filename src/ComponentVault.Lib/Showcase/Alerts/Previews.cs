namespace ComponentVault.Lib.Showcase.Alerts;

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
