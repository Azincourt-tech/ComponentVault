using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Progress;

// snippet:control.progressring
/// <summary>
/// Anel de progresso desenhado com Microsoft.Maui.Graphics. O valor exibido anima até o valor
/// pedido (600ms, sem overshoot) e as cores acompanham o tema.
/// </summary>
public class ProgressRing : GraphicsView, IDrawable
{
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress), typeof(double), typeof(ProgressRing), 0d,
        coerceValue: (_, v) => Math.Clamp((double)v, 0, 1),
        propertyChanged: (b, _, n) => ((ProgressRing)b).AnimateTo((double)n));

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness), typeof(float), typeof(ProgressRing), 8f,
        propertyChanged: (b, _, _) => ((ProgressRing)b).Invalidate());

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor), typeof(Color), typeof(ProgressRing), Color.FromArgb("#E85D04"),
        propertyChanged: (b, _, _) => ((ProgressRing)b).Invalidate());

    private const string AnimationName = "ProgressRingValue";
    private double _shown;

    public ProgressRing()
    {
        Drawable = this;
        WidthRequest = HeightRequest = 96;
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) =>
        {
            this.AbortAnimation(AnimationName);
            ThemeColors.PaletteChanged -= OnThemeChanged;
        };
    }

    /// <summary>Valor de 0 a 1.</summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public float Thickness
    {
        get => (float)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var size = Math.Min(rect.Width, rect.Height) - Thickness;
        var x = rect.Center.X - size / 2;
        var y = rect.Center.Y - size / 2;

        canvas.StrokeSize = Thickness;
        canvas.StrokeLineCap = LineCap.Round;

        // Trilha
        canvas.StrokeColor = ThemeColors.Resolve("SurfaceAlt", Colors.LightGray);
        canvas.DrawEllipse(x, y, size, size);

        if (_shown <= 0.001) return;

        // Arco: começa no topo (90°) e corre no sentido horário.
        canvas.StrokeColor = ProgressColor;
        var sweep = (float)(360 * Math.Min(_shown, 0.9999));
        canvas.DrawArc(x, y, size, size, 90, 90 - sweep, clockwise: true, closed: false);
    }

    private void AnimateTo(double target)
    {
        SemanticProperties.SetDescription(this, $"{target:P0} concluído");
        this.AbortAnimation(AnimationName);
        if (Handler is null)
        {
            _shown = target;
            Invalidate();
            return;
        }

        this.Animate(AnimationName, v => { _shown = v; Invalidate(); },
            _shown, target, length: 600, easing: Easing.CubicOut);
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.progressring
