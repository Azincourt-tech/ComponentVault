using System.Collections.ObjectModel;
using System.ComponentModel;
using ComponentVault.App.Controls;
using ComponentVault.Core.Models;
using ComponentVault.Core.ViewModels;

namespace ComponentVault.App.Views.Detail;

public partial class ComponentDetailPage : ContentPage, IQueryAttributable
{
    private readonly ComponentDetailViewModel _viewModel;
    private readonly SlidingIndicator _tabIndicator;

    public ComponentDetailPage(ComponentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Só animação: o sublinhado acompanha a aba que a ViewModel escolheu.
        _tabIndicator = new SlidingIndicator(TabUnderline, TabRow);
        _tabIndicator.JumpTo(TabFor(viewModel.SelectedTab));
        viewModel.PropertyChanged += OnViewModelChanged;
    }

    // Só repassa: IQueryAttributable é do MAUI e a ViewModel não pode conhecê-lo (RNF 7.1).
    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        _viewModel.OnNavigatedTo(new ReadOnlyDictionary<string, object>(query));

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ComponentDetailViewModel.SelectedTab))
        {
            _tabIndicator.SlideTo(TabFor(_viewModel.SelectedTab));
        }
    }

    private Label TabFor(DetailTab tab) => tab switch
    {
        DetailTab.Compare => CompareTab,
        DetailTab.Docs => DocsTab,
        DetailTab.Code => CodeTab,
        _ => PreviewTab,
    };
}
