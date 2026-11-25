using ComponentVault.Lib.Theming;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ComponentVault.Lib.Showcase.Charts;

[ShowcasePreview("chart.custom")]
public partial class ChartCustomPreview : ContentView
{
    public ChartCustomPreview() => InitializeComponent();
}

[ShowcasePreview("chart.livecharts")]
public partial class ChartLiveChartsPreview : ContentView
{
    public ChartLiveChartsPreview()
    {
        InitializeComponent();
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnThemeChanged; };
        BuildCharts();
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => BuildCharts();

    // snippet:chart.livecharts.code
    private void BuildCharts()
    {
        // Paleta tirada dos tokens do tema atual: destaque para o dado principal, neutros para o resto.
        var accent = Sk("Accent");
        var ink = Sk("Ink");
        var muted = Sk("InkSecondary");
        var grid = Sk("Hairline");
        var neutral = Sk("HairlineStrong");

        string[] months = ["Jun", "Jul", "Ago", "Set", "Out", "Nov"];

        OrdersChart.Series =
        [
            new ColumnSeries<double>
            {
                Name = "Pedidos",
                Values = [168, 182, 175, 201, 214, 236],
                Fill = new SolidColorPaint(accent),
                Rx = 6,
                Ry = 6,
                MaxBarWidth = 28,
            },
            new LineSeries<double>
            {
                Name = "Meta",
                Values = [170, 175, 180, 190, 200, 210],
                Stroke = new SolidColorPaint(ink) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0,
            },
        ];
        OrdersChart.XAxes =
        [
            new Axis
            {
                Labels = months,
                LabelsPaint = new SolidColorPaint(muted),
                TextSize = 13,
                SeparatorsPaint = null,
            },
        ];
        OrdersChart.YAxes =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(muted),
                TextSize = 13,
                MinLimit = 0,
                SeparatorsPaint = new SolidColorPaint(grid) { StrokeThickness = 1 },
            },
        ];
        OrdersChart.LegendPosition = LegendPosition.Hidden;
        OrdersChart.TooltipTextPaint = new SolidColorPaint(ink);
        OrdersChart.TooltipBackgroundPaint = new SolidColorPaint(Sk("Surface"));

        // Rosca com 3 fatias: um tom de destaque e dois neutros, rótulos na legenda (não dentro das fatias).
        (string Name, double Value, SKColor Color)[] channels =
        [
            ("App", 58, accent),
            ("Site", 31, ink),
            ("Loja física", 11, neutral),
        ];

        ChannelChart.Series = channels
            .Select(c => (ISeries)new PieSeries<double>
            {
                Name = $"{c.Name} · {c.Value:0}%",
                Values = [c.Value],
                Fill = new SolidColorPaint(c.Color),
                InnerRadius = 56,
                Pushout = 0,
            })
            .ToArray();
        ChannelChart.LegendPosition = LegendPosition.Right;
        ChannelChart.LegendTextPaint = new SolidColorPaint(ink);
        ChannelChart.LegendTextSize = 14;
    }

    private static SKColor Sk(string token)
    {
        var c = ThemeColors.Resolve(token, Colors.Gray);
        return new SKColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
    // /snippet:chart.livecharts.code
}
