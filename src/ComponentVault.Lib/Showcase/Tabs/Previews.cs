using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Tabs;

[ShowcasePreview("tabs.custom")]
public partial class TabsCustomPreview : ContentView
{
    // snippet:tabs.custom.code
    private int _selected;

    public TabsCustomPreview()
    {
        InitializeComponent();
        TabStrip.SizeChanged += (_, _) => PlaceUnderline(animate: false);
        Select(0, animate: false);
    }

    private Label[] Tabs => [Tab0, Tab1, Tab2];

    private View[] Panels => [Panel0, Panel1, Panel2];

    private void OnTabTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string text && int.TryParse(text, out var index)) Select(index, animate: true);
    }

    private void Select(int index, bool animate)
    {
        _selected = index;
        for (var i = 0; i < Tabs.Length; i++)
        {
            var active = i == index;
            // Selecionada em tinta; as outras em cinza legível (4.5:1), nunca só pela cor do sublinhado.
            Tabs[i].SetThemeColor(Label.TextColorProperty, active ? "Ink" : "InkSecondary");
            SemanticProperties.SetDescription(Tabs[i], active ? $"{Tabs[i].Text}, aba selecionada" : Tabs[i].Text);
            Panels[i].IsVisible = active;
        }

        PlaceUnderline(animate);
    }

    private void PlaceUnderline(bool animate)
    {
        var width = TabStrip.Width / Tabs.Length;
        if (width <= 0) return;

        Underline.WidthRequest = width;
        var x = width * _selected;
        if (animate)
        {
            _ = Underline.TranslateToAsync(x, 0, 200, Easing.CubicOut);
        }
        else
        {
            Underline.TranslationX = x;
        }
    }
    // /snippet:tabs.custom.code
}

[ShowcasePreview("tabs.sharpnado")]
public partial class TabsSharpnadoPreview : ContentView
{
    public TabsSharpnadoPreview() => InitializeComponent();
}

[ShowcasePreview("tabs.syncfusion")]
public partial class TabsSyncfusionPreview : ContentView
{
    public TabsSyncfusionPreview() => InitializeComponent();
}

[ShowcasePreview("tabs.uranium")]
public partial class TabsUraniumPreview : ContentView
{
    public TabsUraniumPreview() => InitializeComponent();
}
