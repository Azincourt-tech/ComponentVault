using System.Collections.ObjectModel;
using System.ComponentModel;
using ComponentVault.App.Controls;
using ComponentVault.Core.Models;
using ComponentVault.Core.ViewModels;
using ComponentVault.Lib.Controls;

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
        Loaded += (_, _) =>
        {
            Hero.PlayInto(Header);
            AttachKeyboard();
            // Aberto direto em apresentação (link com present=true): agora há janela para a tela cheia.
            if (_viewModel.IsPresentationMode) OnPresentationChanged(true);
        };
        Unloaded += (_, _) => DetachKeyboard();
    }

    // Ao sair, guarda onde o cabeçalho está, para a volta: a página de baixo faz o voo inverso.
    // Saindo no meio da apresentação, a janela não pode ficar presa em tela cheia.
    protected override void OnDisappearing()
    {
        if (_viewModel.IsPresentationMode) _viewModel.ExitPresentationCommand.Execute(null);
        Hero.CaptureReturn(Header);
        base.OnDisappearing();
    }

    private void OnBackTapped(object? sender, TappedEventArgs e) => _ = Shell.Current?.GoToAsync("..");

    // Voltar do sistema (Android) durante a apresentação sai dela, e não da página.
    protected override bool OnBackButtonPressed()
    {
        if (!_viewModel.IsPresentationMode) return base.OnBackButtonPressed();
        _viewModel.ExitPresentationCommand.Execute(null);
        return true;
    }

    private void OnPresentationChanged(bool presenting)
    {
        SetFullScreen(presenting);

        // O botão "Apresentar" some ao entrar e leva o foco junto: sem foco, o teclado não chega.
        if (presenting) Dispatcher.Dispatch(() => ExitPresentationButton.Focus());
    }

    // Setas trocam o nível e Esc sai, só durante a apresentação. Teclas que um controle do
    // preview já tratou (setas numa caixa de texto ou num slider) não chegam aqui.
    private bool HandleKey(string key)
    {
        if (!_viewModel.IsPresentationMode) return false;

        switch (key)
        {
            case "Left":
                _viewModel.PreviousVariantCommand.Execute(null);
                return true;
            case "Right":
                _viewModel.NextVariantCommand.Execute(null);
                return true;
            case "Escape":
                _viewModel.ExitPresentationCommand.Execute(null);
                return true;
            default:
                return false;
        }
    }

#if WINDOWS
    private Microsoft.UI.Xaml.UIElement? _keyboardRoot;

    private void AttachKeyboard()
    {
        if (Window?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native || native.Content is not { } root) return;
        _keyboardRoot = root;
        root.KeyDown += OnNativeKeyDown;
    }

    private void DetachKeyboard()
    {
        if (_keyboardRoot is not null) _keyboardRoot.KeyDown -= OnNativeKeyDown;
        _keyboardRoot = null;
    }

    private void OnNativeKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (HandleKey(e.Key.ToString())) e.Handled = true;
    }

    private void SetFullScreen(bool fullScreen)
    {
        if (Window?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native) return;
        // Em tela cheia a barra de título some: a página deixa de reservar o espaço dela.
        Controls.TitleBarOverlay.SetVisible(!fullScreen);
        native.AppWindow.SetPresenter(fullScreen
            ? Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen
            : Microsoft.UI.Windowing.AppWindowPresenterKind.Overlapped);
    }
#else
    private void AttachKeyboard() { }

    private void DetachKeyboard() { }

    private void SetFullScreen(bool fullScreen) { }
#endif

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
        else if (e.PropertyName == nameof(ComponentDetailViewModel.IsPresentationMode))
        {
            OnPresentationChanged(_viewModel.IsPresentationMode);
        }
    }

    // Lado a lado: quantos cards por linha cabe no espaço real (um card precisa de ~380px), não no
    // tipo de aparelho. Cada item inclui os 20px de espaço à direita, e a margem -20 do FlexLayout
    // já deu a ele essa largura extra.
    private const double MinCompareCard = 380;

    private void OnCompareSizeChanged(object? sender, EventArgs e) => SizeCompareItems();

    private void OnCompareChildAdded(object? sender, ElementEventArgs e) => Dispatcher.Dispatch(SizeCompareItems);

    private void SizeCompareItems()
    {
        if (CompareFlex.Width <= 0) return;
        var columns = Math.Clamp((int)(CompareFlex.Width / (MinCompareCard + 20)), 1, 3);
        var width = Math.Floor(CompareFlex.Width / columns);
        foreach (var item in CompareFlex.Children.OfType<View>())
        {
            if (Math.Abs(item.WidthRequest - width) > 0.5) item.WidthRequest = width;
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
