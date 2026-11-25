using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
