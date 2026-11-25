using System.Globalization;
using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Charts;

/// <summary>Base dos minigráficos: lê "Points" como texto ("3 5 4 8") e redesenha ao trocar o tema.</summary>
public abstract class MiniChart : GraphicsView, IDrawable
{
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points), typeof(string), typeof(MiniChart), string.Empty,
        propertyChanged: (b, _, _) => ((MiniChart)b).OnPointsChanged());

    protected MiniChart()
    {
        Drawable = this;
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnThemeChanged; };
    }

    /// <summary>Valores separados por espaço ou vírgula, em ordem cronológica.</summary>
    public string Points
    {
        get => (string)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    protected double[] Values { get; private set; } = [];

    public abstract void Draw(ICanvas canvas, RectF dirtyRect);

    private void OnPointsChanged()
    {
        Values = (Points ?? string.Empty)
            .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
        Invalidate();
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}

// snippet:control.sparkline
/// <summary>Linha de tendência sem eixos: área suave em tinta e o último ponto marcado na cor de destaque.</summary>
public sealed class Sparkline : MiniChart
{
    public override void Draw(ICanvas canvas, RectF rect)
    {
        if (Values.Length < 2) return;

        const float pad = 4;
        var min = Values.Min();
        var range = Math.Max(Values.Max() - min, 0.0001);
        var step = (rect.Width - pad * 2) / (Values.Length - 1);
        PointF At(int i) => new(
            rect.X + pad + i * step,
            rect.Bottom - pad - (float)((Values[i] - min) / range) * (rect.Height - pad * 2));

        var line = new PathF(At(0));
        for (var i = 1; i < Values.Length; i++) line.LineTo(At(i));

        var area = new PathF(line);
        area.LineTo(At(Values.Length - 1).X, rect.Bottom);
        area.LineTo(At(0).X, rect.Bottom);
        area.Close();

        var ink = ThemeColors.Resolve("Ink", Colors.Black);
        canvas.FillColor = ink.WithAlpha(0.06f);
        canvas.FillPath(area);

        canvas.StrokeColor = ink;
        canvas.StrokeSize = 2;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(line);

        var last = At(Values.Length - 1);
        canvas.FillColor = ThemeColors.Resolve("Accent", Colors.Blue);
        canvas.FillCircle(last, 4);
    }
}

/// <summary>Barras compactas: todas neutras, só a última (o período atual) na cor de destaque.</summary>
public sealed class MiniBars : MiniChart
{
    public override void Draw(ICanvas canvas, RectF rect)
    {
        if (Values.Length == 0) return;

        const float gap = 4;
        var max = Math.Max(Values.Max(), 0.0001);
        var width = (rect.Width - gap * (Values.Length - 1)) / Values.Length;
        var neutral = ThemeColors.Resolve("HairlineStrong", Colors.LightGray);
        var accent = ThemeColors.Resolve("Accent", Colors.Blue);

        for (var i = 0; i < Values.Length; i++)
        {
            var height = Math.Max(2f, (float)(Values[i] / max) * rect.Height);
            canvas.FillColor = i == Values.Length - 1 ? accent : neutral;
            canvas.FillRoundedRectangle(rect.X + i * (width + gap), rect.Bottom - height, width, height, 3);
        }
    }
}
// /snippet:control.sparkline
