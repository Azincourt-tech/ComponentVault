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
/// Barra flutuante de vidro com navegação, voltar e tema num lugar só, igual em todas as telas.
/// No desktop fica no topo; no celular, embaixo, ao alcance do polegar. A pílula da seção ativa
/// desliza entre as abas (inclusive ao trocar de página) e o tema é escolhido direto, sem ciclo:
/// os três ficam à vista e o ativo fica marcado.
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

    private static bool IsPhone => DeviceInfo.Idiom == DeviceIdiom.Phone;

    private readonly AppearanceViewModel? _appearance;
    private readonly Border _capsule;
    private readonly SlidingIndicator _navSlider;
    private readonly SlidingIndicator _themeSlider;
    private readonly View _back;
    private readonly View _backDivider;
    private readonly List<(NavSection Section, HorizontalStackLayout View, MauiIcon Icon, Label Label)> _navItems = [];
    private readonly List<(ThemeOption Option, Grid View, MauiIcon Icon)> _themeItems = [];
    private bool _placed;

    public NavDock()
    {
        _appearance = IPlatformApplication.Current?.Services.GetService<AppearanceViewModel>();
        var itemHeight = IsPhone ? 44d : 40d;

        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = IsPhone ? LayoutOptions.End : LayoutOptions.Start;
        Margin = IsPhone ? new Thickness(12, 8, 12, 16) : new Thickness(16, 14, 16, 6);

        // Voltar (só no detalhe)
        _back = IconButton(FluentIcons.ChevronLeft24, "Voltar", itemHeight, OnBackTapped);
        _back.IsVisible = false;
        _backDivider = Divider();
        _backDivider.IsVisible = false;

        // Seções
        var navIndicator = Indicator("Ink", itemHeight);
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
        Content = _capsule;

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

    private void OnLoaded(object? sender, EventArgs e)
    {
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
        // O seletor de aparência e o Shell vivem mais que a página: sem isso ela ficaria presa a eles.
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
        }
    }

    private void OnSectionChanged()
    {
        UpdateNavItems();
        if (_placed) _navSlider.SlideTo(SelectedNavView());
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

        _capsule.Opacity = 0;
        _capsule.TranslationY = IsPhone ? 24 : -18;
        _capsule.Scale = 0.96;
        _ = _capsule.FadeToAsync(1, 420, Easing.CubicOut);
        _ = _capsule.TranslateToAsync(0, 0, 520, Easing.SpringOut);
        _ = _capsule.ScaleToAsync(1, 520, Easing.SpringOut);
    }

    private View? SelectedNavView() => _navItems.Find(i => i.Section == Section).View;

    private View? SelectedThemeView() => _themeItems.Find(i => i.Option.IsSelected).View;

    private void UpdateNavItems()
    {
        foreach (var (section, _, icon, label) in _navItems)
        {
            var selected = section == Section;
            var key = selected ? "OnInk" : "InkSecondary";
            icon.SetThemeColor(MauiIcon.IconColorProperty, key);
            label.SetThemeColor(Label.TextColorProperty, key);
            // No celular só a seção ativa mostra o nome: cabe tudo numa barra só.
            label.IsVisible = !IsPhone || selected;
        }
    }

    private void UpdateThemeItems()
    {
        foreach (var (option, _, icon) in _themeItems)
        {
            icon.SetThemeColor(MauiIcon.IconColorProperty, option.IsSelected ? "Ink" : "InkTertiary");
        }
    }

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
        var size = IsPhone ? 40d : height;
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

    private static Border Indicator(string colorKey, double height)
    {
        var indicator = new Border
        {
            HeightRequest = height,
            WidthRequest = height,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = height / 2 },
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
