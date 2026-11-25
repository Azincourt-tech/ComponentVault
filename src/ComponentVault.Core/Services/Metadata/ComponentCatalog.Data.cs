using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Data() =>
    [
        new()
        {
            Id = "chart",
            Name = "Chart",
            Icon = "DataBarVertical24",
            Category = ComponentCategory.Data,
            Summary = "Gráficos de tendência, comparação e proporção.",
            Tags = ["gráfico", "chart", "kpi", "sparkline", "livecharts", "rosca", "barras"],
            AddedOn = new DateOnly(2026, 11, 24),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem gráficos prontos. Para indicadores pequenos, desenhar com Microsoft.Maui.Graphics é leve e sem dependência; para eixos, legendas e interação, o LiveCharts2 entrega tudo sobre SkiaSharp.",
                Properties =
                [
                    new("Series", "Séries do LiveCharts (ColumnSeries, LineSeries, PieSeries)."),
                    new("XAxes / YAxes", "Rótulos, limites e linhas de grade dos eixos."),
                    new("Points (Sparkline)", "Valores em texto, ex.: \"3 5 4 8\"."),
                ],
                UseCases = ["Indicadores com tendência.", "Comparar períodos e proporções."],
                Limitations = ["Gráficos não substituem o número: mostre o valor em texto também."],
            },
            Variants =
            [
                new()
                {
                    Id = "chart.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Cards de indicador com sparkline e minibarras desenhadas em GraphicsView.",
                    Highlights =
                    [
                        "O número é o protagonista; o gráfico só mostra a tendência, sem eixos.",
                        "Variação com seta e texto, não só pela cor vermelha ou verde.",
                        "Só o período atual na cor de destaque; o resto fica neutro.",
                    ],
                    Snippets = [Xaml("Uso", "chart.custom"), CSharp("Desenho (MiniCharts.cs)", "control.sparkline")],
                },
                new()
                {
                    Id = "chart.livecharts",
                    Title = "LiveCharts2",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "LiveChartsCore.SkiaSharpView.Maui",
                    Summary = "Colunas com linha de meta e gráfico de rosca, nas cores do tema.",
                    Highlights =
                    [
                        "Paleta tirada dos tokens: destaque para o dado, tinta para a meta, neutros para o resto.",
                        "Grade só horizontal e em linha fina; rótulos de 13px legíveis.",
                        "Rosca com legenda ao lado em vez de rótulos espremidos nas fatias.",
                        "Recria as cores ao trocar entre claro e escuro.",
                    ],
                    Snippets = [Xaml("Uso", "chart.livecharts"), CSharp("Séries e eixos", "chart.livecharts.code")],
                },
            ],
        },
    ];
}
