using System.Globalization;
using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.SearchBars;

// snippet:control.expandingsearch
/// <summary>
/// Busca que mora num botão redondo ao lado do título e se abre só quando chamada. Tocar na lupa
/// estica o botão até a largura toda (280ms) enquanto o título sai pela esquerda, e as sugestões
/// descem uma a uma (35ms de intervalo). O trecho que casou fica destacado; acentos e maiúsculas
/// não importam. O X ou escolher uma sugestão fecham na ordem inversa.
/// </summary>
public class ExpandingSearch : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(ExpandingSearch), "", propertyChanged: (b, _, n) => ((ExpandingSearch)b)._title.Text = (string)n);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(ExpandingSearch), "Buscar", propertyChanged: (b, _, n) => ((ExpandingSearch)b)._input.Placeholder = (string)n);

    private const double Collapsed = 48;
    private const string WidthAnimation = "SearchWidth";
    private const int MaxResults = 5;

    private readonly Grid _header;
    private readonly Label _title;
    private readonly Border _pill;
    private readonly BorderlessEntry _input;
    private readonly MauiIcon _close;
    private readonly VerticalStackLayout _results = new() { Spacing = 2 };
    private bool _open;

    /// <summary>Disparado ao escolher uma sugestão (toque ou Enter no primeiro resultado).</summary>
    public event EventHandler<string>? ItemChosen;

    public ExpandingSearch()
    {
        _title = new Label { FontFamily = "JakartaBold", FontSize = 22, VerticalOptions = LayoutOptions.Center };
        _title.SetThemeColor(Label.TextColorProperty, "Ink");

        var lens = new MauiIcon { Icon = FluentIcons.Search24, IconSize = 20, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        lens.SetThemeColor(MauiIcon.IconColorProperty, "Ink");

        _input = new BorderlessEntry
        {
            FontFamily = "JakartaRegular",
            FontSize = 16,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            ReturnType = ReturnType.Search,
            Opacity = 0,
            IsVisible = false,
        };
        _input.SetThemeColor(Entry.TextColorProperty, "Ink");
        _input.SetThemeColor(Entry.PlaceholderColorProperty, "InkTertiary");
        _input.TextChanged += (_, _) => Refresh(animated: false);
        _input.Completed += (_, _) => { if (_results.Children.OfType<Border>().FirstOrDefault() is { BindingContext: string first }) Choose(first); };

        _close = new MauiIcon { Icon = FluentIcons.Dismiss24, IconSize = 18, VerticalOptions = LayoutOptions.Center, Opacity = 0, IsVisible = false };
        _close.SetThemeColor(MauiIcon.IconColorProperty, "InkSecondary");
        SemanticProperties.SetDescription(_close, "Fechar busca");
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => Close();
        _close.GestureRecognizers.Add(closeTap);

        var row = new Grid
        {
            ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
            ColumnSpacing = 10,
            Padding = new Thickness(13, 0, 14, 0),
        };
        row.Add(lens, 0);
        row.Add(_input, 1);
        row.Add(_close, 2);

        _pill = new Border
        {
            WidthRequest = Collapsed,
            HeightRequest = Collapsed,
            HorizontalOptions = LayoutOptions.End,
            StrokeShape = new RoundRectangle { CornerRadius = Collapsed / 2 },
            Padding = 0,
            Content = row,
        };
        FieldFrames.Apply(_pill, false);
        _input.Focused += (_, _) => FieldFrames.Apply(_pill, true);
        _input.Unfocused += (_, _) => FieldFrames.Apply(_pill, false);
        SemanticProperties.SetDescription(_pill, "Buscar");
        var openTap = new TapGestureRecognizer();
        openTap.Tapped += (_, _) => Open();
        _pill.GestureRecognizers.Add(openTap);

        _header = new Grid { HeightRequest = Collapsed, Children = { _title, _pill } };
        _header.SizeChanged += (_, _) => { if (_open && !this.AnimationIsRunning(WidthAnimation)) _pill.WidthRequest = _header.Width; };

        Content = new VerticalStackLayout { Spacing = 12, Children = { _header, _results } };
        _results.IsVisible = false;
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    /// <summary>Itens buscáveis. Sem texto, os primeiros aparecem como sugestões.</summary>
    public IList<string> Items { get; set; } = [];

    public void Open()
    {
        if (_open) return;
        _open = true;
        _input.IsVisible = _close.IsVisible = true;
        Resize(_header.Width);
        _ = _title.FadeToAsync(0, 160);
        _ = _title.TranslateToAsync(-24, 0, 220, Easing.CubicOut);
        _ = _input.FadeToAsync(1, 220);
        _ = _close.FadeToAsync(1, 220);
        _input.Focus();
        Refresh(animated: true);
    }

    public void Close()
    {
        if (!_open) return;
        _open = false;
        _input.Unfocus();
        _input.Text = string.Empty;
        _results.IsVisible = false;
        _input.IsVisible = _close.IsVisible = false;
        _input.Opacity = _close.Opacity = 0;
        Resize(Collapsed);
        _ = _title.FadeToAsync(1, 220);
        _ = _title.TranslateToAsync(0, 0, 220, Easing.CubicOut);
    }

    private void Resize(double to)
    {
        var from = _pill.WidthRequest;
        this.AbortAnimation(WidthAnimation);
        this.Animate(WidthAnimation, v => _pill.WidthRequest = v, from, Math.Max(Collapsed, to), length: 280, easing: Easing.CubicOut);
    }

    private void Choose(string item)
    {
        ItemChosen?.Invoke(this, item);
        Close();
    }

    private void Refresh(bool animated)
    {
        if (!_open) return;
        var term = (_input.Text ?? string.Empty).Trim();
        var compare = CultureInfo.InvariantCulture.CompareInfo;
        const CompareOptions loose = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        var matches = term.Length == 0
            ? Items.Take(MaxResults - 1).Select(i => (Item: i, Index: -1)).ToList()
            : Items.Select(i => (Item: i, Index: compare.IndexOf(i, term, loose)))
                   .Where(x => x.Index >= 0)
                   .OrderBy(x => x.Index).ThenBy(x => x.Item.Length)
                   .Take(MaxResults).ToList();

        _results.Clear();
        _results.Add(Caption(term.Length == 0 ? "Sugestões" : matches.Count == 0 ? $"Nada para “{term}”" : "Resultados"));
        foreach (var (item, index) in matches) _results.Add(Row(item, index, term.Length));
        _results.IsVisible = true;

        if (!animated) return;
        // Cascata: cada linha desce 8px e aparece um pouco depois da anterior.
        for (var i = 0; i < _results.Children.Count; i++)
        {
            if (_results.Children[i] is not View view) continue;
            view.Opacity = 0;
            view.TranslationY = -8;
            var delay = i * 35;
            view.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delay), () =>
            {
                _ = view.FadeToAsync(1, 180);
                _ = view.TranslateToAsync(0, 0, 220, Easing.CubicOut);
            });
        }
    }

    private static Label Caption(string text)
    {
        var label = new Label { Text = text, FontFamily = "JakartaMedium", FontSize = 13, Margin = new Thickness(14, 0, 0, 2) };
        label.SetThemeColor(Label.TextColorProperty, "InkTertiary");
        return label;
    }

    // Linha de resultado: ícone e o nome com o trecho encontrado em negrito na cor de destaque.
    private Border Row(string item, int index, int length)
    {
        var icon = new MauiIcon { Icon = index < 0 ? FluentIcons.Clock20 : FluentIcons.Search20, IconSize = 18, VerticalOptions = LayoutOptions.Center };
        icon.SetThemeColor(MauiIcon.IconColorProperty, "InkTertiary");

        var text = new FormattedString();
        if (index < 0) text.Spans.Add(Part(item, bold: false));
        else
        {
            text.Spans.Add(Part(item[..index], bold: false));
            text.Spans.Add(Part(item.Substring(index, length), bold: true));
            text.Spans.Add(Part(item[(index + length)..], bold: false));
        }
        var name = new Label { FormattedText = text, FontSize = 16, VerticalOptions = LayoutOptions.Center };

        var grid = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 12 };
        grid.Add(icon, 0);
        grid.Add(name, 1);

        var row = new Border
        {
            BindingContext = item,
            Padding = new Thickness(14, 11),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Colors.Transparent,
            Content = grid,
        };
        SemanticProperties.SetDescription(row, item);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Choose(item);
        row.GestureRecognizers.Add(tap);
        var hover = new PointerGestureRecognizer();
        hover.PointerEntered += (_, _) => row.BackgroundColor = ThemeColors.Resolve("SurfaceHover", Colors.LightGray);
        hover.PointerExited += (_, _) => row.BackgroundColor = Colors.Transparent;
        row.GestureRecognizers.Add(hover);
        return row;

        static Span Part(string value, bool bold)
        {
            var span = new Span { Text = value, FontFamily = bold ? "JakartaBold" : "JakartaRegular" };
            span.SetThemeColor(Span.TextColorProperty, bold ? "AccentText" : "Ink");
            return span;
        }
    }
}
// /snippet:control.expandingsearch
