using ComponentVault.Lib.Theming;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
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
        // Uma cor só (o destaque) em intensidades diferentes; o resto vem dos tokens neutros do tema.
        var accent = Sk("Accent");
        var ink = Sk("Ink");
        var muted = Sk("InkTertiary");
        var grid = Sk("Hairline");

        string[] months = ["Jun", "Jul", "Ago", "Set", "Out", "Nov"];

        OrdersChart.Series =
        [
            // Meses anteriores atenuados e o mês atual cheio: o olho vai direto para o dado de agora.
            // Duas séries na mesma posição (IgnoresBarPosition), cada uma com os meses dela.
            new ColumnSeries<double?>
            {
                Name = "Pedidos",
                Values = [168, 182, 175, 201, 214, null],
                Fill = new SolidColorPaint(accent.WithAlpha(110)),
                Rx = 6,
                Ry = 6,
                MaxBarWidth = 32,
                IgnoresBarPosition = true,
            },
            new ColumnSeries<double?>
            {
                Name = "Pedidos (mês atual)",
                Values = [null, null, null, null, null, 236],
                Fill = new SolidColorPaint(accent),
                Rx = 6,
                Ry = 6,
                MaxBarWidth = 32,
                IgnoresBarPosition = true,
            },
            new LineSeries<double>
            {
                Name = "Meta",
                Values = [170, 175, 180, 190, 200, 210],
                Stroke = new SolidColorPaint(ink.WithAlpha(150))
                {
                    StrokeThickness = 1.5f,
                    PathEffect = new DashEffect([5, 5]),
                },
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
                TextSize = 12,
                SeparatorsPaint = null,
            },
        ];
        OrdersChart.YAxes =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(muted),
                TextSize = 12,
                MinLimit = 0,
                MinStep = 50,
                Labeler = value => value.ToString("0"),
                SeparatorsPaint = new SolidColorPaint(grid) { StrokeThickness = 1 },
            },
        ];
        OrdersChart.LegendPosition = LegendPosition.Hidden;
        OrdersChart.TooltipTextPaint = new SolidColorPaint(ink);
        OrdersChart.TooltipBackgroundPaint = new SolidColorPaint(Sk("Ground"));

        // Rosca: o mesmo azul em 100%, 55% e 25%. A legenda fica no XAML, na fonte do app.
        (string Name, double Value, byte Alpha)[] channels =
        [
            ("App", 58, 255),
            ("Site", 31, 140),
            ("Loja física", 11, 64),
        ];

        ChannelChart.Series = channels
            .Select(c => (ISeries)new PieSeries<double>
            {
                Name = c.Name,
                Values = [c.Value],
                Fill = new SolidColorPaint(accent.WithAlpha(c.Alpha)),
                InnerRadius = 64,
                MaxRadialColumnWidth = 22,
                Pushout = 0,
            })
            .ToArray();
        ChannelChart.LegendPosition = LegendPosition.Hidden;
        ChannelChart.TooltipTextPaint = new SolidColorPaint(ink);
        ChannelChart.TooltipBackgroundPaint = new SolidColorPaint(Sk("Ground"));
    }

    private static SKColor Sk(string token)
    {
        var c = ThemeColors.Resolve(token, Colors.Gray);
        return new SKColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
    // /snippet:chart.livecharts.code
}

/// <summary>Ponto de gráfico com a cor própria (PointColorPath).</summary>
public sealed record ChartPoint(string Label, double Value, Brush Color);

[ShowcasePreview("chart.syncfusion")]
public partial class ChartSyncfusionPreview : ContentView
{
    public ChartSyncfusionPreview()
    {
        InitializeComponent();
        Paint();
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Paint();

    // Só o mês atual na cor de destaque; os anteriores em neutro, para o olho ir direto ao que importa.
    private void Paint()
    {
        var accent = new SolidColorBrush(ThemeColors.Resolve("Accent", Colors.OrangeRed));
        var neutral = new SolidColorBrush(ThemeColors.Resolve("HairlineStrong", Colors.LightGray));
        string[] months = ["jun", "jul", "ago", "set", "out", "nov"];
        double[] values = [6.1, 7.4, 6.8, 9.2, 10.5, 12.8];
        DownloadSeries.ItemsSource = months.Select((m, i) => new ChartPoint(m, values[i], i == months.Length - 1 ? accent : neutral)).ToList();

        PlatformSeries.ItemsSource = new List<ChartPoint>
        {
            new("Android", 62, accent),
            new("Windows", 27, new SolidColorBrush(Color.FromArgb("#FAA307"))),
            new("iOS", 11, new SolidColorBrush(ThemeColors.Resolve("InkTertiary", Colors.Gray))),
        };
    }
}

[ShowcasePreview("chart.microcharts")]
public partial class ChartMicrochartsPreview : ContentView
{
    public ChartMicrochartsPreview()
    {
        InitializeComponent();
        Paint();
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnMicroPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnMicroPaletteChanged;
    }

    private void OnMicroPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Paint();

    private static SkiaSharp.SKColor Sk(string key, Color fallback) =>
        SkiaSharp.SKColor.Parse(ThemeColors.Resolve(key, fallback).ToArgbHex(includeAlpha: true));

    // O Microcharts desenha com SkiaSharp: as cores vêm da paleta como SKColor, o fundo fica
    // transparente (o card mostra o dele) e o texto usa a Plus Jakarta carregada no início.
    private void Paint()
    {
        var accent = Sk("Accent", Colors.OrangeRed);
        var ink = Sk("InkSecondary", Colors.Gray);
        var muted = Sk("HairlineStrong", Colors.LightGray);
        var typeface = ComponentVault.Lib.Hosting.VaultHostingExtensions.Typeface;

        float[] visits = [520, 610, 580, 720, 690, 810, 890];

        VisitsChart.Chart = new Microcharts.LineChart
        {
            // Sparkline: sem rótulos. O título do card já diz o período, e 7 nomes de dia não cabem na largura.
            Entries = visits.Select(v => new Microcharts.ChartEntry(v) { Color = accent }),
            LineMode = Microcharts.LineMode.Spline,
            LineSize = 3,
            LineAreaAlpha = 40,
            PointSize = 0,
            EnableYFadeOutGradient = true,
            LabelTextSize = 13,
            LabelOrientation = Microcharts.Orientation.Horizontal,
            BackgroundColor = SkiaSharp.SKColors.Transparent,
            LabelColor = ink,
            Typeface = typeface,
            Margin = 8,
        };

        SourcesChart.Chart = new Microcharts.DonutChart
        {
            Entries =
            [
                new Microcharts.ChartEntry(58) { Label = "Busca", ValueLabel = "58%", Color = accent, TextColor = ink, ValueLabelColor = ink },
                new Microcharts.ChartEntry(27) { Label = "Direto", ValueLabel = "27%", Color = SkiaSharp.SKColor.Parse("#FAA307"), TextColor = ink, ValueLabelColor = ink },
                new Microcharts.ChartEntry(15) { Label = "Redes", ValueLabel = "15%", Color = muted, TextColor = ink, ValueLabelColor = ink },
            ],
            HoleRadius = 0.62f,
            LabelMode = Microcharts.LabelMode.RightOnly,
            LabelTextSize = 13,
            BackgroundColor = SkiaSharp.SKColors.Transparent,
            Typeface = typeface,
            Margin = 6,
        };

        GoalChart.Chart = new Microcharts.RadialGaugeChart
        {
            // Sem legenda: o título do card já diz o valor, e a legenda do medidor ficaria por cima do anel.
            Entries = [new Microcharts.ChartEntry(78) { Color = accent }],
            MinValue = 0,
            MaxValue = 100,
            LineSize = 18,
            LineAreaAlpha = 40,
            LabelTextSize = 13,
            BackgroundColor = SkiaSharp.SKColors.Transparent,
            Typeface = typeface,
            Margin = 6,
        };
    }
}
