using System.ComponentModel;
using ComponentVault.App.Controls;
using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Catalog;

public partial class CatalogPage : ContentPage
{
    private const double ContentWidth = 880;

    private readonly CatalogViewModel _viewModel;
    private bool _appeared;

    public CatalogPage(CatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Só animação: a lista entra em cascata ao abrir e a cada troca de filtro, não a cada tecla.
    // Ao voltar do detalhe a página reaparece, mas a lista já estava lá: entra parada.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_appeared) Cascade.Begin();
        _appeared = true;
        _viewModel.PropertyChanged += OnViewModelChanged;
    }

    protected override void OnDisappearing()
    {
        // A ViewModel é singleton e vive mais que esta página.
        _viewModel.PropertyChanged -= OnViewModelChanged;
        base.OnDisappearing();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogViewModel.SelectedOrigin)) Cascade.Begin();
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
