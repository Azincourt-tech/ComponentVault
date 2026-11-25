using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Pickers;

[ShowcasePreview("picker.base")]
public partial class PickerBasePreview : ContentView
{
    public PickerBasePreview() => InitializeComponent();
}

[ShowcasePreview("picker.styled")]
public partial class PickerStyledPreview : ContentView
{
    public PickerStyledPreview() => InitializeComponent();

    private void OnPlatformChanged(object? sender, EventArgs e) =>
        PlatformHint.Text = PlatformPicker.SelectedItem is string platform
            ? $"O build vai gerar o pacote para {platform}."
            : "Define o alvo do build.";

    private void OnFocusChanged(object? sender, FocusEventArgs e)
    {
        PlatformFrame.SetThemeBrush(Border.StrokeProperty, e.IsFocused ? "Accent" : "HairlineStrong");
        PlatformFrame.StrokeThickness = e.IsFocused ? 2 : 1;
    }
}

[ShowcasePreview("picker.uranium")]
public partial class PickerUraniumPreview : ContentView
{
    public PickerUraniumPreview() => InitializeComponent();
}
