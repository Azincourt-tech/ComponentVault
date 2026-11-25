using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Catalog;

public partial class CatalogPage : ContentPage
{
    private const double ContentWidth = 880;

    public CatalogPage(CatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Só layout: no Windows a CollectionView recorta itens quando tem MaximumWidthRequest,
    // então ela ocupa a largura toda e a margem lateral centraliza a coluna de 880px.
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not View root || root.Width <= 0) return;

        var minimum = DeviceInfo.Idiom == DeviceIdiom.Phone ? 12 : 32;
        var side = Math.Max(minimum, (root.Width - ContentWidth) / 2);
        ResultsList.Margin = new Thickness(side, 0, side, 0);
    }
}
