using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Editors;

[ShowcasePreview("editor.base")]
public partial class EditorBasePreview : ContentView
{
    public EditorBasePreview() => InitializeComponent();
}

[ShowcasePreview("editor.styled")]
public partial class EditorStyledPreview : ContentView
{
    public EditorStyledPreview()
    {
        InitializeComponent();
        UpdateCounter(0);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e) => UpdateCounter(e.NewTextValue?.Length ?? 0);

    private void UpdateCounter(int length)
    {
        var max = DescriptionEditor.MaxLength;
        CounterLabel.Text = $"{length}/{max}";

        var key = length >= max ? "DangerText" : length >= max * 0.9 ? "WarningText" : "InkTertiary";
        CounterLabel.SetThemeColor(Label.TextColorProperty, key);
        SemanticProperties.SetDescription(CounterLabel, $"{max - length} caracteres restantes");
    }

    // Anel de foco de 2px na cor de destaque, igual ao VaultTextField.
    private void OnFocusChanged(object? sender, FocusEventArgs e)
    {
        EditorFrame.SetThemeBrush(Border.StrokeProperty, e.IsFocused ? "Accent" : "HairlineStrong");
        EditorFrame.StrokeThickness = e.IsFocused ? 2 : 1;
    }
}
