using System.ComponentModel;
using ComponentVault.App.Views.Catalog;
using ComponentVault.Core.Navigation;
using ComponentVault.Core.ViewModels;
using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.App.Controls;

public enum NavSection
{
    Home,
    Catalog,
}

/// <summary>
/// Navegação e tema do app, em duas formas conforme a largura disponível (Adaptive):
/// <list type="bullet">
/// <item>Média e expandida (tablet, desktop): cápsula de vidro no topo com voltar, seções com
/// nome e os três temas à vista.</item>
/// <item>Compacta (celular, janela estreita): barra inferior em largura total, ao alcance do
/// polegar, com Início, Catálogo e Tema (ícone e nome embaixo). Tocar em Tema transforma a própria
/// barra no seletor de temas, sem menu solto por cima do conteúdo; escolher volta à navegação.
/// O voltar fica no topo da página, onde se espera no celular.</item>
/// </list>
/// Nas duas, a pílula da seção ativa desliza entre os itens, inclusive ao trocar de página.
/// </summary>
public sealed class NavDock : ContentView
{
    public static readonly BindableProperty SectionProperty = BindableProperty.Create(
        nameof(Section), typeof(NavSection), typeof(NavDock), NavSection.Home,
        propertyChanged: (b, _, _) => ((NavDock)b).OnSectionChanged());

    public static readonly BindableProperty ShowBackProperty = BindableProperty.Create(
        nameof(ShowBack), typeof(bool), typeof(NavDock), false,
        propertyChanged: (b, _, n) => ((NavDock)b).OnShowBackChanged((bool)n));

    // Lembra a última seção entre instâncias: cada página tem a sua barra, e a pílula sai
    // de onde estava na página anterior. A entrada da barra só roda na abertura do app.
    private static NavSection? s_lastSection;
    private static bool s_introPlayed;

    // Toque (alvos de 44px) depende do aparelho; posição e rótulos dependem da largura disponível.
    private static bool IsTouch => DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet;

    private static bool IsCompact => Adaptive.Current.IsCompact;

    private readonly AppearanceViewModel? _appearance;
    private readonly Border _capsule;
    private readonly SlidingIndicator _navSlider;
    private readonly SlidingIndicator _themeSlider;
    private readonly View _back;
    private readonly View _backDivider;
    private readonly List<(NavSection Section, HorizontalStackLayout View, MauiIcon Icon, Label Label)> _navItems = [];
    private readonly List<(ThemeOption Option, Grid View, MauiIcon Icon)> _themeItems = [];
    private bool _placed;

    // Barra inferior da faixa compacta.
    private const double MobileItemHeight = 54;
    private const uint MorphLength = 220;
    private readonly Border _mobileBar;
    private readonly Grid _mobileNav;
    private readonly Grid _mobileThemes;
    private readonly SlidingIndicator _mobileNavSlider;
    private readonly SlidingIndicator _mobileThemeSlider;
    private readonly List<(NavSection Section, View View, MauiIcon Icon, Label Label)> _mobileNavItems = [];
    private readonly List<(ThemeOption Option, View View, MauiIcon Icon, Label Label)> _mobileThemeItems = [];
    private readonly MauiIcon _mobileThemeIcon = new() { IconSize = 22, HorizontalOptions = LayoutOptions.Center };
    private bool _themesOpen;

    public NavDock()
    {
        _appearance = IPlatformApplication.Current?.Services.GetService<AppearanceViewModel>();
        var itemHeight = IsTouch ? 44d : 40d;

        // Voltar (só no detalhe)
        _back = IconButton(FluentIcons.ChevronLeft24, "Voltar", itemHeight, OnBackTapped);
        _back.IsVisible = false;
        _backDivider = Divider();
        _backDivider.IsVisible = false;

        // Seções
        var navIndicator = Indicator("Accent", itemHeight);
        var navRow = new HorizontalStackLayout { Spacing = 2 };
        AddNavItem(navRow, NavSection.Home, FluentIcons.Home24, "Início", itemHeight);
        AddNavItem(navRow, NavSection.Catalog, FluentIcons.Grid24, "Catálogo", itemHeight);
        var navGroup = new Grid { Children = { navIndicator, navRow } };
        _navSlider = new SlidingIndicator(navIndicator, navRow);
        _navSlider.Measured += (_, _) => PlaceOnce();

        // Tema
        var themeIndicator = Indicator("SurfaceHover", itemHeight);
        themeIndicator.SetThemeColor(Border.StrokeProperty, "ChromeStroke");
        themeIndicator.StrokeThickness = 1;
        var themeRow = new HorizontalStackLayout { Spacing = 2 };
        if (_appearance is not null)
        {
            foreach (var option in _appearance.Options) AddThemeItem(themeRow, option, itemHeight);
        }
        var themeGroup = new Grid { Children = { themeIndicator, themeRow } };
        _themeSlider = new SlidingIndicator(themeIndicator, themeRow);

        _capsule = new Border
        {
            Padding = 5,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = (itemHeight / 2) + 5 },
            // Única sombra do app: a barra flutua sobre o conteúdo e precisa se descolar dele.
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.22f, Radius = 30, Offset = new Point(0, 10) },
            Content = new HorizontalStackLayout
            {
                Spacing = 0,
                Children = { _back, _backDivider, navGroup, Divider(), themeGroup },
            },
        };
        _capsule.SetThemeColor(BackgroundColorProperty, "Chrome");
        _capsule.SetThemeColor(Border.StrokeProperty, "ChromeStroke");

        // ===== Barra inferior (compacta) =====
        // Navegação: três colunas iguais; a pílula de tinta marca a seção ativa e desliza.
        var mobileNavIndicator = Indicator("Accent", MobileItemHeight, radius: 18);
        var mobileNavRow = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)] };
        AddMobileNavItem(mobileNavRow, 0, NavSection.Home, FluentIcons.Home24, "Início");
        AddMobileNavItem(mobileNavRow, 1, NavSection.Catalog, FluentIcons.Grid24, "Catálogo");
        AddMobileThemeEntry(mobileNavRow, 2);
        _mobileNav = new Grid { Children = { mobileNavIndicator, mobileNavRow } };
        _mobileNavSlider = new SlidingIndicator(mobileNavIndicator, mobileNavRow);
        _mobileNavSlider.Measured += (_, _) => { if (_mobileNavSlider.Target is null) _mobileNavSlider.JumpTo(SelectedMobileNavView()); };

        // Seletor de tema: a mesma barra, com Claro, Escuro, Vidro e fechar.
        var mobileThemeIndicator = Indicator("SurfaceHover", MobileItemHeight, radius: 18);
        mobileThemeIndicator.SetThemeColor(Border.StrokeProperty, "ChromeStroke");
        mobileThemeIndicator.StrokeThickness = 1;
        var mobileThemeRow = new Grid
        {
            ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(new GridLength(56))],
        };
        if (_appearance is not null)
        {
            for (var i = 0; i < _appearance.Options.Count; i++) AddMobileThemeItem(mobileThemeRow, i, _appearance.Options[i]);
        }
        var close = IconButton(FluentIcons.Dismiss24, "Fechar seletor de tema", MobileItemHeight, () => SetThemesOpen(false));
        Grid.SetColumn(close, 3);
        mobileThemeRow.Add(close);
        _mobileThemes = new Grid { Children = { mobileThemeIndicator, mobileThemeRow }, IsVisible = false, Opacity = 0 };
        _mobileThemeSlider = new SlidingIndicator(mobileThemeIndicator, mobileThemeRow);
        _mobileThemeSlider.Measured += (_, _) => { if (_mobileThemeSlider.Target is null) _mobileThemeSlider.JumpTo(SelectedMobileThemeView()); };

        _mobileBar = new Border
        {
            Padding = 6,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.22f, Radius = 30, Offset = new Point(0, 10) },
            Content = new Grid { Children = { _mobileNav, _mobileThemes } },
        };
        _mobileBar.SetThemeColor(BackgroundColorProperty, "Chrome");
        _mobileBar.SetThemeColor(Border.StrokeProperty, "ChromeStroke");

        ApplyWidthClass();
        UpdateNavItems();
        UpdateThemeItems();
        _themeSlider.JumpTo(SelectedThemeView());

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>Seção desta página. No detalhe é descoberta pela rota (Início ou Catálogo).</summary>
    public NavSection Section
    {
        get => (NavSection)GetValue(SectionProperty);
        set => SetValue(SectionProperty, value);
    }

    /// <summary>Mostra o botão voltar (páginas empilhadas, como o detalhe).</summary>
    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    // Compacta: barra inferior em largura total. Outras faixas: cápsula centrada no topo.
    // Muda ao vivo quando a janela é redimensionada.
    private void ApplyWidthClass()
    {
        var compact = IsCompact;
        VerticalOptions = compact ? LayoutOptions.End : LayoutOptions.Start;
        HorizontalOptions = compact ? LayoutOptions.Fill : LayoutOptions.Center;
        Margin = compact ? new Thickness(12, 6, 12, 12) : new Thickness(16, 14, 16, 6);

        View target = compact ? _mobileBar : _capsule;
        if (!ReferenceEquals(Content, target))
        {
            if (!compact) SetThemesOpen(false, animated: false);
            Content = target;
        }
    }

    private void OnWidthClassChanged(object? sender, EventArgs e)
    {
        ApplyWidthClass();
        UpdateNavItems();
    }

    // Barra compacta: Tema troca a navegação pelos três temas (e volta) na própria barra.
    private void SetThemesOpen(bool open, bool animated = true)
    {
        if (_themesOpen == open && animated) return;
        _themesOpen = open;
        var (show, hide) = open ? (_mobileThemes, _mobileNav) : (_mobileNav, _mobileThemes);

        if (!animated)
        {
            hide.IsVisible = false;
            hide.Opacity = 0;
            show.IsVisible = true;
            show.Opacity = 1;
            show.TranslationY = 0;
            return;
        }

        // Quem sai desce e some; quem entra sobe e aparece. Os dois ao mesmo tempo, rápido.
        show.IsVisible = true;
        show.Opacity = 0;
        show.TranslationY = 10;
        _ = show.FadeToAsync(1, MorphLength, Easing.CubicOut);
        _ = show.TranslateToAsync(0, 0, MorphLength, Easing.CubicOut);
        _ = hide.TranslateToAsync(0, -10, MorphLength, Easing.CubicIn);
        hide.FadeToAsync(0, MorphLength, Easing.CubicIn).ContinueWith(_ =>
            Dispatcher.Dispatch(() =>
            {
                if (_themesOpen != open) return;
                hide.IsVisible = false;
                hide.TranslationY = 0;
            }));

        if (open) Dispatcher.Dispatch(() => _mobileThemeSlider.JumpTo(SelectedMobileThemeView()));
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Adaptive.Current.ClassChanged += OnWidthClassChanged;
        ApplyWidthClass();
        UpdateNavItems();
        if (_appearance is not null) _appearance.PropertyChanged += OnAppearanceChanged;
        UpdateThemeItems();
        _themeSlider.JumpTo(SelectedThemeView());

        if (ShowBack)
        {
            UpdateSectionFromStack();
            // Numa rota com várias páginas (ex.: catalog/component), o Shell só insere as de baixo
            // depois que a de cima carrega: confere de novo quando a navegação termina.
            if (Shell.Current is { } shell) shell.Navigated += OnShellNavigated;
        }

        PlayIntro();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        // O seletor de aparência, o Shell e o Adaptive vivem mais que a página: sem isso ela ficaria presa a eles.
        Adaptive.Current.ClassChanged -= OnWidthClassChanged;
        if (_appearance is not null) _appearance.PropertyChanged -= OnAppearanceChanged;
        if (Shell.Current is { } shell) shell.Navigated -= OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e) => UpdateSectionFromStack();

    // Detalhe: a seção é de onde ele foi aberto, lida na pilha de páginas abaixo da dele.
    private void UpdateSectionFromStack()
    {
        if (Shell.Current?.Navigation.NavigationStack is not { } stack) return;

        Element? page = Parent;
        while (page is not null and not Page) page = page.Parent;
        var below = page is not null && stack.Contains(page) ? stack.TakeWhile(p => p != page) : stack;
        Section = below.Any(p => p is CatalogPage) ? NavSection.Catalog : NavSection.Home;
    }

    private void OnAppearanceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppearanceViewModel.Selected)) return;

        // O tema salvo é lido fora da UI thread, e o aviso pode chegar de lá.
        if (Dispatcher.IsDispatchRequired) Dispatcher.Dispatch(Apply);
        else Apply();

        void Apply()
        {
            UpdateThemeItems();
            _themeSlider.SlideTo(SelectedThemeView());
            _mobileThemeSlider.SlideTo(SelectedMobileThemeView());
        }
    }

    private void OnSectionChanged()
    {
        UpdateNavItems();
        if (_placed) _navSlider.SlideTo(SelectedNavView());
        _mobileNavSlider.SlideTo(SelectedMobileNavView());
    }

    private void OnShowBackChanged(bool show)
    {
        _back.IsVisible = show;
        _backDivider.IsVisible = show;
    }

    // Primeira medição: a pílula da seção parte da posição que tinha na página anterior.
    private void PlaceOnce()
    {
        if (_placed) return;
        var current = SelectedNavView();
        if (current is null || current.Width <= 0) return;

        _placed = true;
        var previous = s_lastSection is { } last && last != Section ? _navItems.Find(i => i.Section == last).View : null;
        if (previous is { Width: > 0 })
        {
            _navSlider.JumpTo(previous);
            _navSlider.SlideTo(current);
        }
        else
        {
            _navSlider.JumpTo(current);
        }
        s_lastSection = Section;
    }

    private void PlayIntro()
    {
        if (s_introPlayed) return;
        s_introPlayed = true;

        var bar = Content;
        if (bar is null) return;
        bar.Opacity = 0;
        bar.TranslationY = IsCompact ? 24 : -18;
        bar.Scale = 0.96;
        _ = bar.FadeToAsync(1, 420, Easing.CubicOut);
        _ = bar.TranslateToAsync(0, 0, 520, Easing.SpringOut);
        _ = bar.ScaleToAsync(1, 520, Easing.SpringOut);
    }

    private View? SelectedNavView() => _navItems.Find(i => i.Section == Section).View;

    private View? SelectedThemeView() => _themeItems.Find(i => i.Option.IsSelected).View;

    private View? SelectedMobileNavView() => _mobileNavItems.Find(i => i.Section == Section).View;

    private View? SelectedMobileThemeView() => _mobileThemeItems.Find(i => i.Option.IsSelected).View;

    private void UpdateNavItems()
    {
        foreach (var (section, _, icon, label) in _navItems)
        {
            var key = section == Section ? "OnAccent" : "InkSecondary";
            icon.SetThemeColor(MauiIcon.IconColorProperty, key);
            label.SetThemeColor(Label.TextColorProperty, key);
        }

        foreach (var (section, _, icon, label) in _mobileNavItems)
        {
            var key = section == Section ? "OnAccent" : "InkSecondary";
            icon.SetThemeColor(MauiIcon.IconColorProperty, key);
            label.SetThemeColor(Label.TextColorProperty, key);
        }
    }

    private void UpdateThemeItems()
    {
        foreach (var (option, _, icon) in _themeItems)
        {
            icon.SetThemeColor(MauiIcon.IconColorProperty, option.IsSelected ? "Ink" : "InkTertiary");
        }

        foreach (var (option, _, icon, label) in _mobileThemeItems)
        {
            var key = option.IsSelected ? "Ink" : "InkSecondary";
            icon.SetThemeColor(MauiIcon.IconColorProperty, key);
            label.SetThemeColor(Label.TextColorProperty, key);
        }

        // O item Tema da barra mostra o ícone do tema atual: dá para saber qual está ativo sem abrir.
        if (_appearance?.Selected is { } current && Enum.TryParse<FluentIcons>(current.Icon, out var glyph))
        {
            _mobileThemeIcon.Icon = glyph;
        }
    }

    // ----- Itens da barra inferior (compacta) -----

    private void AddMobileNavItem(Grid row, int column, NavSection section, FluentIcons glyph, string text)
    {
        var (item, icon, label) = MobileItem(glyph, text);
        item.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => OnSectionTapped(section)) });
        Grid.SetColumn(item, column);
        row.Add(item);
        _mobileNavItems.Add((section, item, icon, label));
    }

    private void AddMobileThemeEntry(Grid row, int column)
    {
        _mobileThemeIcon.SetThemeColor(MauiIcon.IconColorProperty, "InkSecondary");
        var label = MobileLabel("Tema");
        label.SetThemeColor(Label.TextColorProperty, "InkSecondary");
        var item = MobileTile(_mobileThemeIcon, label, "Escolher tema");
        item.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => SetThemesOpen(true)) });
        Grid.SetColumn(item, column);
        row.Add(item);
    }

    private void AddMobileThemeItem(Grid row, int column, ThemeOption option)
    {
        var (item, icon, label) = MobileItem(Enum.TryParse<FluentIcons>(option.Icon, out var glyph) ? glyph : FluentIcons.Color24, option.Label);
        SemanticProperties.SetDescription(item, $"Tema {option.Label.ToLowerInvariant()}");
        item.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                _appearance?.SelectCommand.Execute(option);
                // Deixa ver a pílula chegar no tema escolhido antes de voltar à navegação.
                await Task.Delay(320);
                SetThemesOpen(false);
            }),
        });
        Grid.SetColumn(item, column);
        row.Add(item);
        _mobileThemeItems.Add((option, item, icon, label));
    }

    private static (View Item, MauiIcon Icon, Label Label) MobileItem(FluentIcons glyph, string text)
    {
        var icon = new MauiIcon { Icon = glyph, IconSize = 22, HorizontalOptions = LayoutOptions.Center };
        var label = MobileLabel(text);
        return (MobileTile(icon, label, text), icon, label);
    }

    // Ícone em cima e nome embaixo, centrados, com 54px de altura: alvo de toque confortável.
    private static View MobileTile(MauiIcon icon, Label label, string description)
    {
        var tile = new VerticalStackLayout
        {
            Spacing = 2,
            HeightRequest = MobileItemHeight,
            Padding = new Thickness(0, 7, 0, 0),
            BackgroundColor = Colors.Transparent,
            Children = { icon, label },
        };
        SemanticProperties.SetDescription(tile, description);
        Motion.SetPress(tile, true);
        return tile;
    }

    private static Label MobileLabel(string text) => new()
    {
        Text = text,
        FontFamily = "JakartaMedium",
        FontSize = 12,
        HorizontalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.NoWrap,
    };

    private void AddNavItem(HorizontalStackLayout row, NavSection section, FluentIcons glyph, string text, double height)
    {
        var icon = new MauiIcon { Icon = glyph, IconSize = 18, VerticalOptions = LayoutOptions.Center };
        var label = new Label
        {
            Text = text,
            FontFamily = "JakartaMedium",
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
        };
        var item = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(14, 0, 16, 0),
            HeightRequest = height,
            MinimumWidthRequest = height,
            Children = { icon, label },
        };
        SemanticProperties.SetDescription(item, text);
        ToolTipProperties.SetText(item, text);
        Motion.SetPress(item, true);
        item.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => OnSectionTapped(section)) });

        row.Add(item);
        _navItems.Add((section, item, icon, label));
    }

    private void AddThemeItem(HorizontalStackLayout row, ThemeOption option, double height)
    {
        var icon = new MauiIcon
        {
            Icon = Enum.TryParse<FluentIcons>(option.Icon, out var glyph) ? glyph : FluentIcons.Color24,
            IconSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var size = IsCompact ? 40d : height;
        var item = new Grid { WidthRequest = size, HeightRequest = height, Children = { icon } };
        var description = $"Tema {option.Label.ToLowerInvariant()}";
        SemanticProperties.SetDescription(item, description);
        ToolTipProperties.SetText(item, description);
        Motion.SetPress(item, true);
        item.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => _appearance?.SelectCommand.Execute(option)),
        });

        row.Add(item);
        _themeItems.Add((option, item, icon));
    }

    private static Border Indicator(string colorKey, double height, double? radius = null)
    {
        var indicator = new Border
        {
            HeightRequest = height,
            WidthRequest = height,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = radius ?? height / 2 },
        };
        indicator.SetThemeColor(BackgroundColorProperty, colorKey);
        return indicator;
    }

    private static Grid IconButton(FluentIcons glyph, string description, double height, Action onTap)
    {
        var icon = new MauiIcon
        {
            Icon = glyph,
            IconSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        icon.SetThemeColor(MauiIcon.IconColorProperty, "Ink");
        var button = new Grid { WidthRequest = height, HeightRequest = height, Children = { icon } };
        SemanticProperties.SetDescription(button, description);
        ToolTipProperties.SetText(button, description);
        Motion.SetPress(button, true);
        button.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return button;
    }

    private static BoxView Divider()
    {
        var divider = new BoxView
        {
            WidthRequest = 1,
            HeightRequest = 20,
            Margin = new Thickness(6, 0),
            VerticalOptions = LayoutOptions.Center,
        };
        divider.SetThemeColor(BoxView.ColorProperty, "ChromeStroke");
        return divider;
    }

    private void OnSectionTapped(NavSection section)
    {
        if (Shell.Current is not { } shell) return;
        if (section == Section && !ShowBack) return;

        // No detalhe, a seção ativa é a página logo abaixo dele: voltar uma página mantém a
        // rolagem e o filtro dela e permite a transição de volta.
        if (section == Section)
        {
            _ = shell.GoToAsync("..");
            return;
        }

        // Rota absoluta: desempilha o que estiver por cima (o detalhe, por exemplo).
        _ = shell.GoToAsync(section == NavSection.Home ? Routes.Home : Routes.CatalogRoot);
    }

    private static void OnBackTapped()
    {
        if (Shell.Current is { } shell) _ = shell.GoToAsync("..");
    }
}
