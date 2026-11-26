using MauiIcons.Core;
using Syncfusion.Maui.Toolkit.Expander;

namespace ComponentVault.Lib.Showcase.Expanders;

[ShowcasePreview("expander.base")]
public partial class ExpanderBasePreview : ContentView
{
    public ExpanderBasePreview() => InitializeComponent();

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        Details.IsVisible = !Details.IsVisible;
        ToggleButton.Text = Details.IsVisible ? "Esconder detalhes do pedido" : "Mostrar detalhes do pedido";
    }
}

[ShowcasePreview("expander.styled")]
public partial class ExpanderStyledPreview : ContentView
{
    public ExpanderStyledPreview() => InitializeComponent();

    // O cabeçalho passa o corpo da seção como parâmetro; a seta do próprio cabeçalho gira junto.
    private async void OnHeaderTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not View body || sender is not Grid header) return;
        var open = !body.IsVisible;
        var chevron = header.Children.OfType<MauiIcon>().First();
        _ = chevron.RotateToAsync(open ? 180 : 0, 200, Easing.CubicOut);
        SemanticProperties.SetHint(header, open ? "Aberta. Toque para fechar" : "Fechada. Toque para abrir");

        if (open)
        {
            body.Opacity = 0;
            body.IsVisible = true;
            await body.FadeToAsync(1, 180);
        }
        else
        {
            body.IsVisible = false;
        }
    }
}

[ShowcasePreview("expander.custom")]
public partial class ExpanderCustomPreview : ContentView
{
    public ExpanderCustomPreview() => InitializeComponent();
}

[ShowcasePreview("expander.toolkit")]
public partial class ExpanderToolkitPreview : ContentView
{
    public ExpanderToolkitPreview() => InitializeComponent();
}

[ShowcasePreview("expander.syncfusion")]
public partial class ExpanderSyncfusionPreview : ContentView
{
    public ExpanderSyncfusionPreview() => InitializeComponent();

    private void OnExpandedChanged(object? sender, ExpandedAndCollapsedEventArgs e) =>
        SemanticProperties.SetHint(Filters, Filters.IsExpanded ? "Aberto" : "Fechado");

    // O resumo no cabeçalho conta os filtros ligados: dá para saber sem abrir.
    private void OnFilterToggled(object? sender, ToggledEventArgs e)
    {
        if (FilterSummary is null || Filters.Content is not Layout list) return;
        var active = list.Children.OfType<Grid>().SelectMany(g => g.Children.OfType<Switch>()).Count(s => s.IsToggled);
        FilterSummary.Text = active switch { 0 => "Nenhum", 1 => "1 ativo", _ => $"{active} ativos" };
    }
}

[ShowcasePreview("expander.accordion")]
public partial class ExpanderAccordionPreview : ContentView
{
    public ExpanderAccordionPreview() => InitializeComponent();
}
