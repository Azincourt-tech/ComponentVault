using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.CheckBoxes;

// snippet:control.animatedcheck
/// <summary>
/// Caixa de seleção desenhada com Microsoft.Maui.Graphics. Ao marcar, o preenchimento cresce do
/// centro e o traço do check é desenhado de ponta a ponta (220ms); ao desmarcar, o caminho volta.
/// Tem o estado indeterminado (um traço) para "selecionar tudo" com parte marcada.
/// </summary>
public class AnimatedCheck : GraphicsView, IDrawable
{
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(AnimatedCheck), false, BindingMode.TwoWay,
        propertyChanged: (b, o, n) => ((AnimatedCheck)b).OnStateChanged());

    public static readonly BindableProperty IsIndeterminateProperty = BindableProperty.Create(
        nameof(IsIndeterminate), typeof(bool), typeof(AnimatedCheck), false,
        propertyChanged: (b, _, _) => ((AnimatedCheck)b).OnStateChanged());

    private const string AnimationName = "AnimatedCheck";

    // Pontas do check, proporcionais à caixa: canto de baixo e ponta de cima.
    private static readonly PointF[] CheckPoints = [new(0.26f, 0.52f), new(0.43f, 0.69f), new(0.75f, 0.35f)];

    private double _shown;

    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    public AnimatedCheck()
    {
        Drawable = this;
        WidthRequest = HeightRequest = 28;
        BackgroundColor = Colors.Transparent;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Toggle();
        GestureRecognizers.Add(tap);
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnPaletteChanged; this.AbortAnimation(AnimationName); };
        UpdateSemantics();
    }

    public bool IsChecked { get => (bool)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }

    /// <summary>Parte marcada (ex.: "selecionar tudo" com alguns itens). Tocar marca tudo.</summary>
    public bool IsIndeterminate { get => (bool)GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }

    public void Toggle()
    {
        if (!IsEnabled) return;
        var next = IsIndeterminate || !IsChecked;
        IsIndeterminate = false;
        IsChecked = next;
    }

    private void OnStateChanged()
    {
        var target = IsChecked || IsIndeterminate ? 1d : 0d;
        UpdateSemantics();
        CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(IsChecked));
        this.AbortAnimation(AnimationName);
        if (Handler is null) { _shown = target; Invalidate(); return; }
        this.Animate(AnimationName, v => { _shown = v; Invalidate(); }, _shown, target, length: 220, easing: Easing.CubicOut);
    }

    private void UpdateSemantics() => SemanticProperties.SetHint(this,
        IsIndeterminate ? "Parcialmente marcado" : IsChecked ? "Marcado" : "Desmarcado");

    public void Draw(ICanvas canvas, RectF rect)
    {
        const float size = 22, radius = 6;
        var box = new RectF(rect.Center.X - size / 2, rect.Center.Y - size / 2, size, size);
        var accent = ThemeColors.Resolve("Accent", Colors.OrangeRed);
        var t = (float)_shown;

        // Caixa vazia: fundo do card e contorno forte, para ser vista sem depender da cor.
        canvas.FillColor = ThemeColors.Resolve("Surface", Colors.White);
        canvas.FillRoundedRectangle(box, radius);
        canvas.StrokeColor = ThemeColors.Resolve("HairlineStrong", Colors.Gray);
        canvas.StrokeSize = 1.5f;
        canvas.DrawRoundedRectangle(box.Inflate(-0.75f, -0.75f), radius);

        if (t <= 0.001f) return;

        // Preenchimento crescendo do centro (até 100% na metade da animação).
        var grow = Math.Min(1, t * 2);
        var filled = new RectF(box.Center.X - size * grow / 2, box.Center.Y - size * grow / 2, size * grow, size * grow);
        canvas.FillColor = IsEnabled ? accent : accent.WithAlpha(0.45f);
        canvas.FillRoundedRectangle(filled, radius * grow);

        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2.2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        if (IsIndeterminate)
        {
            var half = size * 0.26f * t;
            canvas.DrawLine(box.Center.X - half, box.Center.Y, box.Center.X + half, box.Center.Y);
            return;
        }

        // Traço do check desenhado até a fração "t" do comprimento total.
        var points = CheckPoints.Select(p => new PointF(box.X + p.X * size, box.Y + p.Y * size)).ToArray();
        var lengths = points.Zip(points.Skip(1), (a, b) => a.Distance(b)).ToArray();
        var remaining = lengths.Sum() * t;
        var path = new PathF(points[0]);
        for (var i = 0; i < lengths.Length && remaining > 0; i++)
        {
            var part = Math.Min(1, remaining / lengths[i]);
            path.LineTo(points[i].X + (points[i + 1].X - points[i].X) * part, points[i].Y + (points[i + 1].Y - points[i].Y) * part);
            remaining -= lengths[i];
        }
        canvas.DrawPath(path);
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.animatedcheck
