using ComponentVault.Lib.Theming;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Carousels;

[ShowcasePreview("carousel.base")]
public partial class CarouselBasePreview : ContentView
{
    public CarouselBasePreview() => InitializeComponent();
}

public sealed record OnboardingSlide(FluentIcons Icon, string Title, string Description, string Duration);

[ShowcasePreview("carousel.styled")]
public partial class CarouselStyledPreview : ContentView
{
    private static readonly OnboardingSlide[] Slides =
    [
        new(FluentIcons.Person24, "Complete seu perfil", "Foto e cargo ajudam a equipe a reconhecer você.", "1 min"),
        new(FluentIcons.PeopleTeam24, "Convide a equipe", "Quem entra pelo convite já cai no espaço certo.", "2 min"),
        new(FluentIcons.Folder24, "Importe seus arquivos", "Arraste uma pasta ou conecte o armazenamento.", "3 min"),
        new(FluentIcons.Rocket24, "Publique o primeiro projeto", "Comece de um modelo ou do zero.", "5 min"),
    ];

    // snippet:carousel.styled.code
    private const double DotSize = 8;
    private const double ActiveDotWidth = 24;

    public CarouselStyledPreview()
    {
        InitializeComponent();
        Carousel.ItemsSource = Slides;

        for (var i = 0; i < Slides.Length; i++)
        {
            var index = i;
            var dot = new Border
            {
                HeightRequest = DotSize,
                WidthRequest = DotSize,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = DotSize / 2 },
            };
            SemanticProperties.SetDescription(dot, $"Ir para o slide {index + 1}");
            dot.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Carousel.ScrollTo(index)) });
            Dots.Children.Add(dot);
        }

        UpdateIndicator(0, animate: false);
    }

    private void OnPositionChanged(object? sender, PositionChangedEventArgs e) => UpdateIndicator(e.CurrentPosition, animate: true);

    private void OnPreviousClicked(object? sender, EventArgs e) => Carousel.ScrollTo(Math.Max(0, Carousel.Position - 1));

    private void OnNextClicked(object? sender, EventArgs e) => Carousel.ScrollTo(Math.Min(Slides.Length - 1, Carousel.Position + 1));

    private void UpdateIndicator(int position, bool animate)
    {
        PositionLabel.Text = $"{position + 1} de {Slides.Length}";
        PreviousButton.IsEnabled = position > 0;
        NextButton.IsEnabled = position < Slides.Length - 1;

        for (var i = 0; i < Dots.Children.Count; i++)
        {
            if (Dots.Children[i] is not Border dot) continue;

            var active = i == position;
            dot.SetThemeColor(BackgroundColorProperty, active ? "Ink" : "HairlineStrong");
            var target = active ? ActiveDotWidth : DotSize;

            if (!animate || dot.Handler is null)
            {
                dot.WidthRequest = target;
                continue;
            }

            // 200ms, sem bounce: a barra cresce no ponto atual e encolhe no anterior.
            var from = dot.WidthRequest;
            dot.Animate($"dot{i}", v => dot.WidthRequest = v, from, target, length: 200, easing: Easing.CubicOut);
        }
    }
    // /snippet:carousel.styled.code
}
