using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Icons;

/// <summary>Desenho do <see cref="AnimatedIcon"/>: cada um tem dois estados e a transição entre eles.</summary>
public enum AnimatedGlyph
{
    /// <summary>Três traços que viram um X (abrir e fechar menu).</summary>
    Menu,

    /// <summary>Triângulo que se divide em duas barras (tocar e pausar).</summary>
    PlayPause,

    /// <summary>Coração vazado que enche com um pulo e uma explosão de pontos (curtir).</summary>
    Heart,
}

// snippet:control.animatedicon
/// <summary>
/// Ícone desenhado com Microsoft.Maui.Graphics que se transforma em vez de trocar de glifo. O
/// estado vai de 0 a 1 (<see cref="IsActive"/>) em 320ms, e cada desenho interpola os próprios
/// pontos: os traços do menu giram até virar um X, as metades do triângulo se afastam até virar
/// as barras da pausa, e o coração enche, pula e solta seis pontos ao redor.
/// </summary>
public class AnimatedIcon : GraphicsView, IDrawable
{
    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph), typeof(AnimatedGlyph), typeof(AnimatedIcon), AnimatedGlyph.Menu,
        propertyChanged: (b, _, _) => ((AnimatedIcon)b).Invalidate());

    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(AnimatedIcon), false, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((AnimatedIcon)b).OnActiveChanged());

    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize), typeof(double), typeof(AnimatedIcon), 28d,
        propertyChanged: (b, _, n) => ((AnimatedIcon)b).WidthRequest = ((AnimatedIcon)b).HeightRequest = (double)n + 20);

    private const string AnimationName = "AnimatedIcon";
    private double _t;      // 0 = estado inicial, 1 = ativo
    private double _burst;  // explosão do coração, de 0 a 1, só ao curtir

    public event EventHandler? Toggled;

    public AnimatedIcon()
    {
        Drawable = this;
        WidthRequest = HeightRequest = 48;
        BackgroundColor = Colors.Transparent;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => IsActive = !IsActive;
        GestureRecognizers.Add(tap);
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnPaletteChanged; this.AbortAnimation(AnimationName); };
    }

    public AnimatedGlyph Glyph { get => (AnimatedGlyph)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }

    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }

    public double IconSize { get => (double)GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }

    private void OnActiveChanged()
    {
        var target = IsActive ? 1d : 0d;
        var from = _t;
        Toggled?.Invoke(this, EventArgs.Empty);
        this.AbortAnimation(AnimationName);
        if (Handler is null) { _t = target; Invalidate(); return; }

        var burst = IsActive && Glyph == AnimatedGlyph.Heart;
        this.Animate(AnimationName, p =>
        {
            _t = from + ((target - from) * Easing.CubicInOut.Ease(p));
            _burst = burst ? p : 0;
            Invalidate();
        }, length: burst ? 520u : 320u, easing: Easing.Linear, finished: (_, _) => { _burst = 0; Invalidate(); });
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var size = (float)IconSize;
        var box = new RectF(rect.Center.X - (size / 2), rect.Center.Y - (size / 2), size, size);
        var ink = ThemeColors.Resolve("Ink", Colors.Black);
        var t = (float)_t;

        canvas.StrokeColor = ink;
        canvas.FillColor = ink;
        canvas.StrokeSize = Math.Max(2, size / 12);
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        switch (Glyph)
        {
            case AnimatedGlyph.Menu: DrawMenu(canvas, box, t); break;
            case AnimatedGlyph.PlayPause: DrawPlayPause(canvas, box, t); break;
            default: DrawHeart(canvas, box, t); break;
        }
    }

    // Traços de cima e de baixo vão ao centro e giram ±45°; o do meio encolhe até sumir.
    private static void DrawMenu(ICanvas canvas, RectF box, float t)
    {
        var half = box.Width * 0.38f;
        var gap = box.Height * 0.26f * (1 - t);
        var angle = 45f * t;
        var c = box.Center;

        canvas.SaveState();
        canvas.Rotate(angle, c.X, c.Y);
        canvas.DrawLine(c.X - half, c.Y - gap, c.X + half, c.Y - gap);
        canvas.RestoreState();

        canvas.SaveState();
        canvas.Rotate(-angle, c.X, c.Y);
        canvas.DrawLine(c.X - half, c.Y + gap, c.X + half, c.Y + gap);
        canvas.RestoreState();

        var middle = half * (1 - (t * 1.6f));
        if (middle > 0) canvas.DrawLine(c.X - middle, c.Y, c.X + middle, c.Y);
    }

    // O triângulo é feito de duas metades (quadriláteros). Na pausa, cada metade vira uma barra.
    private static void DrawPlayPause(ICanvas canvas, RectF box, float t)
    {
        float L = box.Left + (box.Width * 0.22f), R = box.Right - (box.Width * 0.12f);
        float T = box.Top + (box.Height * 0.14f), B = box.Bottom - (box.Height * 0.14f), M = box.Center.Y;
        float mid = (L + R) / 2;

        // Play: metade esquerda (L,T)-(mid,meio de cima)-(mid,meio de baixo)-(L,B); direita fecha a ponta.
        float topMid = T + ((M - T) * (mid - L) / (R - L)), bottomMid = B - ((B - M) * (mid - L) / (R - L));
        PointF[] playLeft = [new(L, T), new(mid, topMid), new(mid, bottomMid), new(L, B)];
        PointF[] playRight = [new(mid, topMid), new(R, M), new(R, M), new(mid, bottomMid)];

        // Pausa: duas barras com um vão no meio.
        float barW = box.Width * 0.24f, cx = box.Center.X, gap = box.Width * 0.11f;
        PointF[] pauseLeft = [new(cx - gap - barW, T), new(cx - gap, T), new(cx - gap, B), new(cx - gap - barW, B)];
        PointF[] pauseRight = [new(cx + gap, T), new(cx + gap + barW, T), new(cx + gap + barW, B), new(cx + gap, B)];

        Fill(canvas, Lerp(playLeft, pauseLeft, t));
        Fill(canvas, Lerp(playRight, pauseRight, t));

        static PointF[] Lerp(PointF[] a, PointF[] b, float t) =>
            a.Select((p, i) => new PointF(p.X + ((b[i].X - p.X) * t), p.Y + ((b[i].Y - p.Y) * t))).ToArray();

        static void Fill(ICanvas canvas, PointF[] points)
        {
            var path = new PathF(points[0]);
            foreach (var p in points.Skip(1)) path.LineTo(p);
            path.Close();
            canvas.FillPath(path);
            // Contorno fino arredonda os cantos e cobre a costura entre as metades.
            canvas.StrokeSize = 2;
            canvas.DrawPath(path);
        }
    }

    // Contorno que enche de vermelho; no meio da animação pula (escala 1,25) e solta pontos.
    private void DrawHeart(ICanvas canvas, RectF box, float t)
    {
        var red = ThemeColors.Resolve("Danger", Colors.Red);
        var scale = 1 + (0.25f * MathF.Sin(MathF.PI * Math.Clamp(t * 1.2f, 0, 1)) * (_burst > 0 ? 1 : 0));
        var c = box.Center;

        canvas.SaveState();
        canvas.Scale(scale, scale);
        var s = 1 / scale;
        var heart = HeartPath(new RectF((c.X * s) - (box.Width / 2), (c.Y * s) - (box.Height / 2), box.Width, box.Height));
        if (t > 0.01f)
        {
            canvas.FillColor = red.WithAlpha(t);
            canvas.FillPath(heart);
        }
        if (t > 0.5f) canvas.StrokeColor = red;
        canvas.DrawPath(heart);
        canvas.RestoreState();

        if (_burst is > 0 and < 1)
        {
            // Seis pontos saem do centro e somem; cores alternadas da paleta.
            var b = (float)_burst;
            var distance = box.Width * (0.45f + (0.35f * b));
            for (var i = 0; i < 6; i++)
            {
                var angle = (i * MathF.PI / 3) - (MathF.PI / 2);
                canvas.FillColor = (i % 2 == 0 ? red : ThemeColors.Resolve("Accent", Colors.Orange)).WithAlpha(1 - b);
                canvas.FillCircle(c.X + (MathF.Cos(angle) * distance), c.Y + (MathF.Sin(angle) * distance), 2.5f * (1 - (b * 0.6f)));
            }
        }
    }

    private static PathF HeartPath(RectF r)
    {
        float x = r.X, y = r.Y, w = r.Width, h = r.Height;
        var path = new PathF(x + (w * 0.5f), y + (h * 0.88f));
        path.CurveTo(x + (w * 0.1f), y + (h * 0.62f), x - (w * 0.02f), y + (h * 0.3f), x + (w * 0.22f), y + (h * 0.16f));
        path.CurveTo(x + (w * 0.36f), y + (h * 0.08f), x + (w * 0.47f), y + (h * 0.16f), x + (w * 0.5f), y + (h * 0.28f));
        path.CurveTo(x + (w * 0.53f), y + (h * 0.16f), x + (w * 0.64f), y + (h * 0.08f), x + (w * 0.78f), y + (h * 0.16f));
        path.CurveTo(x + (w * 1.02f), y + (h * 0.3f), x + (w * 0.9f), y + (h * 0.62f), x + (w * 0.5f), y + (h * 0.88f));
        path.Close();
        return path;
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.animatedicon
