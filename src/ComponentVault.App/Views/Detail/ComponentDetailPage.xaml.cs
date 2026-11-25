using System.Collections.ObjectModel;
using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Detail;

public partial class ComponentDetailPage : ContentPage, IQueryAttributable
{
    private readonly ComponentDetailViewModel _viewModel;

    public ComponentDetailPage(ComponentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Só repassa: IQueryAttributable é do MAUI e a ViewModel não pode conhecê-lo (RNF 7.1).
    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        _viewModel.OnNavigatedTo(new ReadOnlyDictionary<string, object>(query));
}
