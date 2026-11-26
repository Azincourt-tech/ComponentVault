using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.CheckBoxes;

[ShowcasePreview("checkbox.base")]
public partial class CheckBoxBasePreview : ContentView
{
    public CheckBoxBasePreview() => InitializeComponent();
}

[ShowcasePreview("checkbox.styled")]
public partial class CheckBoxStyledPreview : ContentView
{
    private static Geometry Parse(string data) => (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data)!;

    public CheckBoxStyledPreview()
    {
        InitializeComponent();
        UpdateSelectAll();
    }

    private CheckBox[] Items => [EmailCheck, DigestCheck, ReleaseCheck];

    private void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is CheckBox { IsEnabled: true } box) box.IsChecked = !box.IsChecked;
    }

    // Selecionar tudo: se todos já estão marcados, desmarca; senão, marca todos.
    private void OnSelectAllTapped(object? sender, TappedEventArgs e)
    {
        var target = !Items.All(i => i.IsChecked);
        foreach (var item in Items) item.IsChecked = target;
    }

    private void OnItemChanged(object? sender, CheckedChangedEventArgs e) => UpdateSelectAll();

    private void UpdateSelectAll()
    {
        // CheckedChanged dispara durante o InitializeComponent, antes de todos os campos existirem.
        if (EmailCheck is null || DigestCheck is null || ReleaseCheck is null || CountLabel is null) return;

        var count = Items.Count(i => i.IsChecked);
        var all = count == Items.Length;

        CountLabel.Text = $"{count} de {Items.Length}";
        SelectAllMark.Data = count == 0 ? null : Parse(all ? "M1.5 5.5L4.5 8.5L9.5 2.5" : "M2 5.5L9 5.5");

        if (count == 0)
        {
            SelectAllBox.BackgroundColor = Colors.Transparent;
            SelectAllBox.SetThemeBrush(Border.StrokeProperty, "HairlineStrong");
        }
        else
        {
            SelectAllBox.SetThemeColor(BackgroundColorProperty, "Accent");
            SelectAllBox.SetThemeBrush(Border.StrokeProperty, "Accent");
        }

        SemanticProperties.SetDescription(SelectAllBox,
            count == 0 ? "Nenhum selecionado" : all ? "Todos selecionados" : $"{count} de {Items.Length} selecionados");
    }
}

[ShowcasePreview("checkbox.uranium")]
public partial class CheckBoxUraniumPreview : ContentView
{
    public CheckBoxUraniumPreview() => InitializeComponent();
}

[ShowcasePreview("checkbox.custom")]
public partial class CheckBoxCustomPreview : ContentView
{
    public CheckBoxCustomPreview()
    {
        InitializeComponent();
        UpdateAll();
    }

    private AnimatedCheck[] Items => [CameraCheck, NotifyCheck, FilesCheck];

    // A linha inteira marca o item (CommandParameter aponta o check da linha).
    private void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not AnimatedCheck check) return;
        check.Toggle();
        UpdateAll();
    }

    // "Todas": se alguma está desmarcada, marca todas; se todas estão marcadas, desmarca todas.
    private void OnAllTapped(object? sender, TappedEventArgs e)
    {
        var next = !Items.All(c => c.IsChecked);
        foreach (var check in Items) check.IsChecked = next;
        UpdateAll();
    }

    private void UpdateAll()
    {
        var marked = Items.Count(c => c.IsChecked);
        AllCheck.IsIndeterminate = marked > 0 && marked < Items.Length;
        AllCheck.IsChecked = marked == Items.Length;
    }
}
