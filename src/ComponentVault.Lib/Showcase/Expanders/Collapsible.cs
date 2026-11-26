using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Expanders;

// snippet:control.collapsible
/// <summary>
/// Seção em cartão que abre e fecha com a altura animada (260ms): o conteúdo é medido e o
/// recorte cresce até ele, em vez de surgir de uma vez e empurrar a tela. O "+" desenhado com
/// Microsoft.Maui.Graphics gira e vira "−", e aberta a seção ganha a borda na cor de destaque.
/// O resumo no cabeçalho (preço, contagem) fica visível mesmo fechada.
/// </summary>
[ContentProperty(nameof(Body))]
public class Collapsible : ContentView, IDrawable
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(Collapsible), "", propertyChanged: (b, _, n) => ((Collapsible)b).OnTextChanged());

    public static readonly BindableProperty SummaryProperty = BindableProperty.Create(
        nameof(Summary), typeof(string), typeof(Collapsible), "", propertyChanged: (b, _, n) => ((Collapsible)b).OnTextChanged());

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(Collapsible), propertyChanged: (b, _, n) => ((Collapsible)b)._body.Content = (View?)n);

    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(Collapsible), false, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((Collapsible)b).OnExpandedChanged(animated: true));

    private const string HeightAnimation = "CollapsibleHeight";
    private const string IconAnimation = "CollapsibleIcon";

    private readonly Border _card;
    private readonly Label _title;
    private readonly Label _summary;
    private readonly GraphicsView _icon;
    private readonly Grid _clip;
    private readonly ContentView _body = new();
    private float _t;   // 0 = "+" (fechada), 1 = "−" (aberta)

    public event EventHandler? ExpandedChanged;

    public Collapsible()
    {
        _title = new Label { FontFamily = "JakartaSemiBold", FontSize = 16, VerticalOptions = LayoutOptions.Center };
        _title.SetThemeColor(Label.TextColorProperty, "Ink");
        _summary = new Label { FontFamily = "JakartaMedium", FontSize = 14, VerticalOptions = LayoutOptions.Center };
        _summary.SetThemeColor(Label.TextColorProperty, "InkSecondary");
        _icon = new GraphicsView { Drawable = this, WidthRequest = 28, HeightRequest = 28, BackgroundColor = Colors.Transparent, InputTransparent = true };

        var header = new Grid
        {
            ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto)],
            ColumnSpacing = 12,
            Padding = new Thickness(18, 14, 14, 14),
            MinimumHeightRequest = 56,
            BackgroundColor = Colors.Transparent,
        };
        header.Add(_title, 0);
        header.Add(_summary, 1);
        header.Add(_icon, 2);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => IsExpanded = !IsExpanded;
        header.GestureRecognizers.Add(tap);

        _body.Padding = new Thickness(18, 0, 18, 18);
        _clip = new Grid { IsClippedToBounds = true, HeightRequest = 0, Children = { _body } };
        _body.VerticalOptions = LayoutOptions.Start;

        _card = new Border { Content = new VerticalStackLayout { Children = { header, _clip } } };
        _card.SetDynamicResource(StyleProperty, "Card");

        Content = _card;
        OnExpandedChanged(animated: false);
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>Texto curto à direita do título, visível mesmo fechada (ex.: "R$ 29/mês").</summary>
    public string Summary { get => (string)GetValue(SummaryProperty); set => SetValue(SummaryProperty, value); }

    public View? Body { get => (View?)GetValue(BodyProperty); set => SetValue(BodyProperty, value); }

    public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    private void OnTextChanged()
    {
        _title.Text = Title;
        _summary.Text = Summary;
        UpdateSemantics();
    }

    private void OnExpandedChanged(bool animated)
    {
        var open = IsExpanded;
        _card.SetThemeBrush(Border.StrokeProperty, open ? "Accent" : "CardStroke");
        _summary.SetThemeColor(Label.TextColorProperty, open ? "AccentText" : "InkSecondary");
        UpdateSemantics();
        ExpandedChanged?.Invoke(this, EventArgs.Empty);

        this.AbortAnimation(HeightAnimation);
        this.AbortAnimation(IconAnimation);
        if (!animated || Handler is null || Width <= 0)
        {
            _clip.HeightRequest = open ? -1 : 0;
            _body.Opacity = open ? 1 : 0;
            _t = open ? 1 : 0;
            _icon.Invalidate();
            return;
        }

        // Mede o conteúdo na largura atual para saber até onde o recorte deve crescer.
        var from = _clip.Height;
        var to = open ? _body.Measure(_clip.Width > 0 ? _clip.Width : Width, double.PositiveInfinity).Height : 0;
        var bodyFrom = _body.Opacity;
        var iconFrom = _t;
        this.Animate(HeightAnimation, p =>
        {
            _clip.HeightRequest = from + ((to - from) * p);
            _body.Opacity = bodyFrom + (((open ? 1 : 0) - bodyFrom) * p);
            _body.TranslationY = open ? -8 * (1 - p) : 0;
        }, length: 260, easing: Easing.CubicOut, finished: (_, cancelled) =>
        {
            // Aberta, volta à altura automática: se o conteúdo mudar, a seção acompanha.
            if (!cancelled && open) _clip.HeightRequest = -1;
        });
        this.Animate(IconAnimation, p => { _t = (float)(iconFrom + (((open ? 1 : 0) - iconFrom) * p)); _icon.Invalidate(); },
            length: 260, easing: Easing.CubicOut);
    }

    private void UpdateSemantics()
    {
        SemanticProperties.SetDescription(this, $"{Title}, {Summary}, {(IsExpanded ? "aberta" : "fechada")}");
        SemanticProperties.SetHint(this, IsExpanded ? "Toque para fechar" : "Toque para abrir");
    }

    // "+" num círculo: a barra vertical gira 90° e encolhe até sumir, deixando o "−".
    public void Draw(ICanvas canvas, RectF rect)
    {
        var c = rect.Center;
        canvas.FillColor = ThemeColors.Resolve(_t > 0.5f ? "AccentTint" : "SurfaceAlt", Colors.LightGray);
        canvas.FillCircle(c.X, c.Y, 14);

        canvas.StrokeColor = ThemeColors.Resolve(_t > 0.5f ? "AccentText" : "InkSecondary", Colors.Gray);
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;
        const float half = 5.5f;
        canvas.DrawLine(c.X - half, c.Y, c.X + half, c.Y);

        var vertical = half * (1 - _t);
        if (vertical <= 0.2f) return;
        canvas.SaveState();
        canvas.Rotate(90 * _t, c.X, c.Y);
        canvas.DrawLine(c.X, c.Y - vertical, c.X, c.Y + vertical);
        canvas.RestoreState();
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => _icon.Invalidate();
}

/// <summary>
/// Pilha de <see cref="Collapsible"/> em que só uma fica aberta: abrir uma fecha a anterior,
/// para a lista não crescer sem fim. Com <see cref="AllowMultiple"/> elas abrem de forma independente.
/// </summary>
public class AccordionStack : VerticalStackLayout
{
    public AccordionStack()
    {
        Spacing = 10;
        ChildAdded += (_, e) => { if (e.Element is Collapsible item) item.ExpandedChanged += OnItemExpanded; };
        ChildRemoved += (_, e) => { if (e.Element is Collapsible item) item.ExpandedChanged -= OnItemExpanded; };
    }

    public bool AllowMultiple { get; set; }

    private void OnItemExpanded(object? sender, EventArgs e)
    {
        if (AllowMultiple || sender is not Collapsible { IsExpanded: true } opened) return;
        foreach (var other in Children.OfType<Collapsible>().Where(c => c != opened && c.IsExpanded)) other.IsExpanded = false;
    }
}
// /snippet:control.collapsible
