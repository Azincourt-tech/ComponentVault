using MauiIcons.Fluent;
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

    private void OnPlatformChanged(object? sender, EventArgs e)
    {
        var platform = PlatformPicker.SelectedItem as string;
        PlatformHint.Text = platform is null ? "Define o alvo do build." : $"O build vai gerar o pacote para {platform}.";
        // O ícone acompanha a escolha: celular, computador ou Mac.
        PlatformIcon.Icon = platform switch
        {
            "Android" or "iOS" => FluentIcons.Phone20,
            "Windows" => FluentIcons.Desktop20,
            "macOS" => FluentIcons.DesktopMac20,
            _ => FluentIcons.PhoneLaptop20,
        };
    }

    private void OnFocusChanged(object? sender, FocusEventArgs e) => FieldFrames.Apply(PlatformFrame, e.IsFocused);
}

[ShowcasePreview("picker.uranium")]
public partial class PickerUraniumPreview : ContentView
{
    public PickerUraniumPreview() => InitializeComponent();
}

[ShowcasePreview("picker.syncfusion")]
public partial class PickerSyncfusionPreview : ContentView
{
    private static readonly string[] Sizes = ["PP", "P", "M", "G", "GG"];
    private static readonly string[] ColorNames = ["Areia", "Coral", "Grafite", "Oliva", "Vinho"];

    public PickerSyncfusionPreview()
    {
        InitializeComponent();
        SizePicker.Columns.Add(new Syncfusion.Maui.Toolkit.Picker.PickerColumn { HeaderText = "Tamanho", ItemsSource = Sizes, SelectedIndex = 2 });
        SizePicker.Columns.Add(new Syncfusion.Maui.Toolkit.Picker.PickerColumn { HeaderText = "Cor", ItemsSource = ColorNames, SelectedIndex = 1 });
        UpdateLabel();
    }

    private void OnSelectionChanged(object? sender, EventArgs e) => UpdateLabel();

    private void UpdateLabel()
    {
        if (SizePicker.Columns.Count < 2) return;
        ChoiceLabel.Text = $"{Sizes[SizePicker.Columns[0].SelectedIndex]} · {ColorNames[SizePicker.Columns[1].SelectedIndex]}";
    }
}
