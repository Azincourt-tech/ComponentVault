using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Popups;

// snippet:control.bottomsheet
/// <summary>
/// Folha inferior: o fundo escurece e a folha sobe da borda de baixo (260ms). Fecha tocando fora,
/// arrastando a alça para baixo além de um terço da altura, ou por <see cref="Close"/>. Fica por cima
/// do conteúdo do container onde é colocada (mesma célula do Grid), não da janela inteira.
/// </summary>
[ContentProperty(nameof(SheetContent))]
public class BottomSheet : Grid
{
    public static readonly BindableProperty SheetContentProperty = BindableProperty.Create(
        nameof(SheetContent), typeof(View), typeof(BottomSheet), propertyChanged: (b, _, n) => ((BottomSheet)b)._body.Content = (View?)n);

    private readonly BoxView _scrim;
    private readonly Border _sheet;
    private readonly ContentView _body = new();
    private bool _open;

    public event EventHandler? Closed;

    public BottomSheet()
    {
        IsVisible = false;

        _scrim = new BoxView { Color = Colors.Black, Opacity = 0 };
        var outside = new TapGestureRecognizer();
        outside.Tapped += (_, _) => Close();
        _scrim.GestureRecognizers.Add(outside);

        // Alça: faixa de 28px de altura para arrastar, mais fácil de pegar que o traço fino.
        var handle = new Grid
        {
            HeightRequest = 28,
            Children =
            {
                new Border
                {
                    WidthRequest = 40, HeightRequest = 5, StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 3 },
                    HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center,
                },
            },
        };
        ((Border)handle.Children[0]).SetThemeColor(BackgroundColorProperty, "HairlineStrong");
        var drag = new PanGestureRecognizer();
        drag.PanUpdated += OnDrag;
        handle.GestureRecognizers.Add(drag);

        _sheet = new Border
        {
            VerticalOptions = LayoutOptions.End,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Padding = new Thickness(20, 0, 20, 24),
            Content = new VerticalStackLayout { Children = { handle, _body } },
        };
        _sheet.SetThemeColor(BackgroundColorProperty, "SurfaceRaised");
        _sheet.SetThemeBrush(Border.StrokeProperty, "Hairline");

        Children.Add(_scrim);
        Children.Add(_sheet);
    }

    public View? SheetContent { get => (View?)GetValue(SheetContentProperty); set => SetValue(SheetContentProperty, value); }

    public bool IsOpen => _open;

    public async void Open()
    {
        if (_open) return;
        _open = true;
        IsVisible = true;
        // Mede a folha antes de animar: ela começa logo abaixo da borda.
        await Task.Yield();
        _sheet.TranslationY = Math.Max(_sheet.Height, 300);
        await Task.WhenAll(
            _scrim.FadeToAsync(0.45, 220),
            _sheet.TranslateToAsync(0, 0, 260, Easing.CubicOut));
        SemanticScreenReader.Announce("Opções de compartilhamento abertas");
    }

    public async void Close()
    {
        if (!_open) return;
        _open = false;
        await Task.WhenAll(
            _scrim.FadeToAsync(0, 180),
            _sheet.TranslateToAsync(0, Math.Max(_sheet.Height, 300), 200, Easing.CubicIn));
        IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // Arrastar para baixo acompanha o dedo; passou de um terço da altura, fecha; senão volta.
    private void OnDrag(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                _sheet.TranslationY = Math.Max(0, e.TotalY);
                _scrim.Opacity = 0.45 * (1 - Math.Min(1, _sheet.TranslationY / Math.Max(1, _sheet.Height)));
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_sheet.TranslationY > _sheet.Height / 3) Close();
                else
                {
                    _ = _sheet.TranslateToAsync(0, 0, 180, Easing.CubicOut);
                    _ = _scrim.FadeToAsync(0.45, 180);
                }
                break;
        }
    }
}
// /snippet:control.bottomsheet
