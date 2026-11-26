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
    private readonly SlidingIndicator _tierIndicator;

    public ComponentDetailPage(ComponentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Só animação: o sublinhado e a pílula acompanham o que a ViewModel escolheu.
        _tabIndicator = new SlidingIndicator(TabUnderline, TabRow);
        _tabIndicator.JumpTo(TabFor(viewModel.SelectedTab));

        // Os níveis chegam com a navegação: a pílula encaixa no primeiro layout e desliza depois.
        _tierIndicator = new SlidingIndicator(TierPill, TierRow);
        _tierIndicator.Measured += (_, _) =>
        {
            if (_tierIndicator.Target is null) _tierIndicator.JumpTo(SelectedTierView());
        };

        viewModel.PropertyChanged += OnViewModelChanged;

        // Ícone e título saem da linha tocada no catálogo (ou nos recentes da Home).
        Loaded += (_, _) => Hero.PlayInto(Header);
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
        else if (e.PropertyName == nameof(ComponentDetailViewModel.SelectedVariant))
        {
            _tierIndicator.SlideTo(SelectedTierView());
        }
    }

    private View? SelectedTierView() =>
        TierRow.Children.OfType<View>().FirstOrDefault(v => v.BindingContext is VariantOption { IsSelected: true });

    private Label TabFor(DetailTab tab) => tab switch
    {
        DetailTab.Compare => CompareTab,
        DetailTab.Docs => DocsTab,
        DetailTab.Code => CodeTab,
        _ => PreviewTab,
    };
}
