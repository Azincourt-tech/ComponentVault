using ComponentVault.App.Controls;
using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Só animação: voltando de um detalhe aberto pelos recentes, ícone e título descem para a linha.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Hero.PlayReturn(Root);
    }
}
