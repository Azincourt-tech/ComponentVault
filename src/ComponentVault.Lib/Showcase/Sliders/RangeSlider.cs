using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Sliders;

// snippet:control.rangeslider
/// <summary>
/// Slider de faixa (mínimo e máximo) desenhado com Microsoft.Maui.Graphics. O polegar mais
/// próximo do toque é o que se move; os valores encaixam no Step e nunca se cruzam.
/// </summary>
public class RangeSlider : GraphicsView, IDrawable
{
    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(RangeSlider), 0d, propertyChanged: Redraw);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(RangeSlider), 100d, propertyChanged: Redraw);

    public static readonly BindableProperty LowerValueProperty = BindableProperty.Create(
        nameof(LowerValue), typeof(double), typeof(RangeSlider), 20d, BindingMode.TwoWay, propertyChanged: OnValueChanged);

    public static readonly BindableProperty UpperValueProperty = BindableProperty.Create(
        nameof(UpperValue), typeof(double), typeof(RangeSlider), 80d, BindingMode.TwoWay, propertyChanged: OnValueChanged);

    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(double), typeof(RangeSlider), 1d);

    private const float ThumbRadius = 11f;
    private const float TrackHeight = 4f;
    private bool? _draggingLower;

    public RangeSlider()
    {
        Drawable = this;
        HeightRequest = 44;
        StartInteraction += (_, e) => Begin(e.Touches[0].X);
        DragInteraction += (_, e) => Move(e.Touches[0].X);
        EndInteraction += (_, _) => { _draggingLower = null; Invalidate(); };
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnThemeChanged; };
    }

    public event EventHandler? ValuesChanged;

    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public double LowerValue { get => (double)GetValue(LowerValueProperty); set => SetValue(LowerValueProperty, value); }

    public double UpperValue { get => (double)GetValue(UpperValueProperty); set => SetValue(UpperValueProperty, value); }

    public double Step { get => (double)GetValue(StepProperty); set => SetValue(StepProperty, value); }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var accent = ThemeColors.Resolve("Accent", Color.FromArgb("#2F6FE4"));
        var track = ThemeColors.Resolve("HairlineStrong", Colors.LightGray);
        var cy = rect.Center.Y;
        var lowerX = XFor(LowerValue, rect);
        var upperX = XFor(UpperValue, rect);

        // Trilha inteira e trecho selecionado.
        canvas.FillColor = track;
        canvas.FillRoundedRectangle(rect.X + ThumbRadius, cy - TrackHeight / 2, rect.Width - ThumbRadius * 2, TrackHeight, TrackHeight / 2);
        canvas.FillColor = accent;
        canvas.FillRectangle(lowerX, cy - TrackHeight / 2, upperX - lowerX, TrackHeight);

        DrawThumb(canvas, lowerX, cy, accent, _draggingLower == true);
        DrawThumb(canvas, upperX, cy, accent, _draggingLower == false);
    }

    private static void DrawThumb(ICanvas canvas, float x, float cy, Color accent, bool active)
    {
        if (active)
        {
            // Halo no polegar ativo: mostra qual dos dois está sendo arrastado.
            canvas.FillColor = accent.WithAlpha(0.18f);
            canvas.FillCircle(x, cy, ThumbRadius + 7);
        }

        canvas.SaveState();
        canvas.SetShadow(new SizeF(0, 1), 2, Colors.Black.WithAlpha(0.18f));
        canvas.FillColor = Colors.White;
        canvas.FillCircle(x, cy, ThumbRadius);
        canvas.RestoreState();

        canvas.StrokeColor = accent;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(x, cy, ThumbRadius - 1);
    }

    private void Begin(float x)
    {
        if (!IsEnabled) return;

        var rect = new RectF(0, 0, (float)Width, (float)Height);
        var toLower = Math.Abs(x - XFor(LowerValue, rect));
        var toUpper = Math.Abs(x - XFor(UpperValue, rect));
        // Empate (polegares juntos): decide pelo lado do toque.
        _draggingLower = toLower < toUpper || (toLower == toUpper && x < XFor(LowerValue, rect));
        Move(x);
    }

    private void Move(float x)
    {
        if (_draggingLower is not { } lower) return;

        var value = ValueFor(x);
        if (lower) LowerValue = Math.Min(value, UpperValue);
        else UpperValue = Math.Max(value, LowerValue);
    }

    private float XFor(double value, RectF rect)
    {
        var range = Maximum - Minimum;
        var t = range <= 0 ? 0 : (value - Minimum) / range;
        return rect.X + ThumbRadius + (float)Math.Clamp(t, 0, 1) * (rect.Width - ThumbRadius * 2);
    }

    private double ValueFor(float x)
    {
        var usable = Width - ThumbRadius * 2;
        var t = usable <= 0 ? 0 : Math.Clamp((x - ThumbRadius) / usable, 0, 1);
        var raw = Minimum + t * (Maximum - Minimum);
        return Step > 0 ? Math.Round(raw / Step) * Step : raw;
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var slider = (RangeSlider)bindable;
        SemanticProperties.SetDescription(slider, $"De {slider.LowerValue:0} a {slider.UpperValue:0}");
        slider.ValuesChanged?.Invoke(slider, EventArgs.Empty);
        slider.Invalidate();
    }

    private static void Redraw(BindableObject bindable, object oldValue, object newValue) => ((RangeSlider)bindable).Invalidate();

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.rangeslider
