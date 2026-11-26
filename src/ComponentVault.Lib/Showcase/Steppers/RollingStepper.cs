using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Steppers;

// snippet:control.rollingstepper
/// <summary>
/// Contador com "–" e "+" redondos e o número que rola: ao aumentar, o valor antigo sai para cima e o
/// novo entra por baixo (o contrário ao diminuir), em 200ms. No limite, o botão fica apagado e
/// para de responder, em vez de sumir.
/// </summary>
public class RollingStepper : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(int), typeof(RollingStepper), 0, BindingMode.TwoWay,
        coerceValue: (b, v) => Math.Clamp((int)v, ((RollingStepper)b).Minimum, ((RollingStepper)b).Maximum),
        propertyChanged: (b, o, n) => ((RollingStepper)b).OnValueChanged((int)o, (int)n));

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(int), typeof(RollingStepper), 0, propertyChanged: (b, _, _) => ((RollingStepper)b).UpdateButtons());

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(int), typeof(RollingStepper), 10, propertyChanged: (b, _, _) => ((RollingStepper)b).UpdateButtons());

    private const double RollDistance = 26;
    private readonly Border _minus;
    private readonly Border _plus;
    private readonly Grid _window;
    private Label _current;

    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    public RollingStepper()
    {
        _minus = RoundButton("–", -1, "Diminuir");
        _plus = RoundButton("+", +1, "Aumentar");
        _current = NumberLabel(Value);

        // Janela com recorte: o número que sai e o que entra só aparecem dentro dela.
        _window = new Grid { WidthRequest = 44, HeightRequest = 36, IsClippedToBounds = true, Children = { _current } };

        Content = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children = { _minus, _window, _plus },
        };
        UpdateButtons();
    }

    public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    public int Minimum { get => (int)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

    public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    private Border RoundButton(string glyph, int step, string description)
    {
        var button = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = new Label
            {
                Text = glyph,
                FontFamily = "JakartaSemiBold",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
        };
        button.SetThemeBrush(Border.StrokeProperty, "HairlineStrong");
        button.SetThemeColor(BackgroundColorProperty, "Surface");
        ((Label)button.Content).SetThemeColor(Label.TextColorProperty, "AccentText");
        SemanticProperties.SetDescription(button, description);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Value += step;
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private static Label NumberLabel(int value)
    {
        var label = new Label
        {
            Text = value.ToString(),
            FontFamily = "JakartaSemiBold",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        label.SetThemeColor(Label.TextColorProperty, "Ink");
        return label;
    }

    private async void OnValueChanged(int oldValue, int newValue)
    {
        UpdateButtons();
        SemanticProperties.SetDescription(this, newValue.ToString());
        ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue, newValue));
        if (_window is null || oldValue == newValue) return;

        // Antes de estar na tela (valor vindo do XAML), troca direto, sem rolar.
        if (Handler is null)
        {
            _current.Text = newValue.ToString();
            return;
        }

        // Subindo: o novo vem de baixo e o antigo sai por cima; descendo, o contrário.
        var direction = newValue > oldValue ? 1 : -1;
        var outgoing = _current;
        var incoming = NumberLabel(newValue);
        incoming.TranslationY = RollDistance * direction;
        _window.Children.Add(incoming);
        _current = incoming;

        await Task.WhenAll(
            outgoing.TranslateToAsync(0, -RollDistance * direction, 200, Easing.CubicOut),
            incoming.TranslateToAsync(0, 0, 200, Easing.CubicOut));
        _window.Children.Remove(outgoing);
    }

    private void UpdateButtons()
    {
        if (_minus is null || _plus is null) return;
        SetEnabled(_minus, Value > Minimum);
        SetEnabled(_plus, Value < Maximum);
    }

    // No limite o botão fica apagado (e não responde), mas continua no lugar: o layout não pula.
    private static void SetEnabled(Border button, bool enabled)
    {
        button.IsEnabled = enabled;
        button.Opacity = enabled ? 1 : 0.35;
    }
}
// /snippet:control.rollingstepper
