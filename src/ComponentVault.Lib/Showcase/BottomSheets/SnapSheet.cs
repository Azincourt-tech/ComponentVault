using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.BottomSheets;

/// <summary>Pontos de parada do <see cref="SnapSheet"/>.</summary>
public enum SheetStop
{
    /// <summary>Só o cabeçalho aparece (resumo).</summary>
    Peek,

    /// <summary>Metade da altura: resumo e começo da lista, com o mapa ainda visível.</summary>
    Half,

    /// <summary>Quase a tela toda; o fundo escurece.</summary>
    Full,
}

// snippet:control.snapsheet
/// <summary>
/// Painel inferior com três pontos de parada, como nos apps de mapa: recolhido (só o resumo),
/// meia altura e cheio. Arrastar o cabeçalho acompanha o dedo; ao soltar, o painel vai para a
/// parada seguinte na direção do arrasto (se passou de 40px) ou volta para a mais próxima. Tocar
/// no cabeçalho sobe uma parada. Cheio, o fundo escurece e tocar nele volta à meia altura.
/// </summary>
[ContentProperty(nameof(SheetContent))]
public class SnapSheet : Grid
{
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header), typeof(View), typeof(SnapSheet), propertyChanged: (b, _, n) => ((SnapSheet)b)._header.Content = (View?)n);

    public static readonly BindableProperty SheetContentProperty = BindableProperty.Create(
        nameof(SheetContent), typeof(View), typeof(SnapSheet), propertyChanged: (b, _, n) => ((SnapSheet)b)._body.Content = (View?)n);

    public static readonly BindableProperty StopProperty = BindableProperty.Create(
        nameof(Stop), typeof(SheetStop), typeof(SnapSheet), SheetStop.Peek, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((SnapSheet)b).OnStopChanged());

    private const double PeekHeight = 96;
    private const double FullInset = 28;     // espaço acima da folha cheia: o contexto não some de vez
    private const double SwitchDistance = 40;
    private const string MoveAnimation = "SnapSheetMove";

    private readonly BoxView _scrim;
    private readonly Border _sheet;
    private readonly ContentView _header = new();
    private readonly ContentView _body = new();
    private double _startY;

    public event EventHandler? StopChanged;

    public SnapSheet()
    {
        _scrim = new BoxView { Color = Colors.Black, Opacity = 0, InputTransparent = true };
        var scrimTap = new TapGestureRecognizer();
        scrimTap.Tapped += (_, _) => Stop = SheetStop.Half;
        _scrim.GestureRecognizers.Add(scrimTap);

        var grabber = new Border
        {
            WidthRequest = 40,
            HeightRequest = 5,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 6),
        };
        grabber.SetThemeColor(BackgroundColorProperty, "HairlineStrong");

        // Área de arrasto: alça e cabeçalho. A lista embaixo continua rolando normalmente.
        var handle = new VerticalStackLayout { Children = { grabber, _header }, BackgroundColor = Colors.Transparent };
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        handle.GestureRecognizers.Add(pan);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Stop = Stop switch { SheetStop.Peek => SheetStop.Half, SheetStop.Half => SheetStop.Full, _ => SheetStop.Peek };
        handle.GestureRecognizers.Add(tap);

        var layout = new Grid { RowDefinitions = [new(GridLength.Auto), new(GridLength.Star)] };
        layout.Add(handle, 0, 0);
        layout.Add(_body, 0, 1);

        _sheet = new Border
        {
            VerticalOptions = LayoutOptions.End,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Padding = new Thickness(20, 0, 20, 0),
            Content = layout,
        };
        _sheet.SetThemeColor(BackgroundColorProperty, "SurfaceRaised");
        _sheet.SetThemeBrush(Border.StrokeProperty, "Hairline");

        Children.Add(_scrim);
        Children.Add(_sheet);
        SizeChanged += (_, _) => Place(animated: false);
        UpdateSemantics();
    }

    public View? Header { get => (View?)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

    public View? SheetContent { get => (View?)GetValue(SheetContentProperty); set => SetValue(SheetContentProperty, value); }

    public SheetStop Stop { get => (SheetStop)GetValue(StopProperty); set => SetValue(StopProperty, value); }

    // A folha tem sempre a altura de "cheia"; as paradas são deslocamentos (TranslationY) para baixo.
    private double FullHeight => Math.Max(PeekHeight, Height - FullInset);

    private double OffsetFor(SheetStop stop) => stop switch
    {
        SheetStop.Full => 0,
        SheetStop.Half => Math.Max(0, FullHeight - (Height * 0.5)),
        _ => FullHeight - PeekHeight,
    };

    private void OnStopChanged()
    {
        Place(animated: true);
        UpdateSemantics();
        StopChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Place(bool animated)
    {
        if (Height <= 0) return;
        _sheet.HeightRequest = FullHeight;
        var target = OffsetFor(Stop);
        this.AbortAnimation(MoveAnimation);
        if (!animated)
        {
            Move(target);
            return;
        }
        var from = _sheet.TranslationY;
        this.Animate(MoveAnimation, v => Move(v), from, target, length: 280, easing: Easing.CubicOut);
    }

    // Move a folha e acerta o fundo: escurece só entre a meia altura e a cheia.
    private void Move(double offset)
    {
        _sheet.TranslationY = offset;
        var half = OffsetFor(SheetStop.Half);
        var progress = half <= 0 ? 0 : Math.Clamp(1 - (offset / half), 0, 1);
        _scrim.Opacity = 0.4 * progress;
        _scrim.InputTransparent = progress < 0.5;
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                this.AbortAnimation(MoveAnimation);
                _startY = _sheet.TranslationY;
                break;
            case GestureStatus.Running:
                Move(Math.Clamp(_startY + e.TotalY, 0, OffsetFor(SheetStop.Peek)));
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                Settle();
                break;
        }
    }

    // Arrastou bem numa direção: vai para a parada seguinte nessa direção. Senão, a mais próxima.
    private void Settle()
    {
        var offset = _sheet.TranslationY;
        var stops = new[] { SheetStop.Full, SheetStop.Half, SheetStop.Peek };
        SheetStop next;
        if (Math.Abs(offset - _startY) > SwitchDistance)
        {
            next = offset < _startY
                ? stops.Where(s => OffsetFor(s) < _startY - 1).OrderByDescending(OffsetFor).FirstOrDefault(SheetStop.Full)
                : stops.Where(s => OffsetFor(s) > _startY + 1).OrderBy(OffsetFor).FirstOrDefault(SheetStop.Peek);
        }
        else
        {
            next = stops.OrderBy(s => Math.Abs(OffsetFor(s) - offset)).First();
        }

        if (next == Stop) Place(animated: true);
        else Stop = next;
    }

    private void UpdateSemantics() => SemanticProperties.SetDescription(_header, Stop switch
    {
        SheetStop.Peek => "Painel recolhido. Toque para abrir até a metade",
        SheetStop.Half => "Painel na metade. Toque para abrir inteiro",
        _ => "Painel aberto. Toque para recolher",
    });
}
// /snippet:control.snapsheet
