using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Otp;

// snippet:control.otp
/// <summary>
/// Código de verificação em caixas. Um único Entry invisível recebe a digitação (e o colar de um
/// código inteiro); as caixas só desenham os dígitos. Assim teclado, colar e leitor de tela
/// funcionam como num campo normal.
/// </summary>
public class OtpInput : ContentView
{
    public static readonly BindableProperty LengthProperty = BindableProperty.Create(
        nameof(Length), typeof(int), typeof(OtpInput), 6,
        propertyChanged: (b, _, _) => ((OtpInput)b).BuildBoxes());

    public static readonly BindableProperty CodeProperty = BindableProperty.Create(
        nameof(Code), typeof(string), typeof(OtpInput), string.Empty, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((OtpInput)b).OnCodeChanged());

    public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(
        nameof(HasError), typeof(bool), typeof(OtpInput), false,
        propertyChanged: (b, _, _) => ((OtpInput)b).Render());

    private readonly HorizontalStackLayout _boxes = new() { Spacing = 8, InputTransparent = true };
    private readonly BorderlessEntry _input = new()
    {
        Keyboard = Keyboard.Numeric,
        Opacity = 0.01,               // invisível, mas ainda focável e clicável
        TextColor = Colors.Transparent,
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill,
    };

    public OtpInput()
    {
        _input.TextChanged += OnInputTextChanged;
        _input.Focused += (_, _) => Render();
        _input.Unfocused += (_, _) => Render();
        SemanticProperties.SetDescription(_input, "Código de verificação");

        Content = new Grid { Children = { _boxes, _input }, HorizontalOptions = LayoutOptions.Start };
        BuildBoxes();
    }

    /// <summary>Disparado quando todos os dígitos foram preenchidos.</summary>
    public event EventHandler<string>? Completed;

    public int Length { get => (int)GetValue(LengthProperty); set => SetValue(LengthProperty, value); }

    public string Code { get => (string)GetValue(CodeProperty); set => SetValue(CodeProperty, value); }

    public bool HasError { get => (bool)GetValue(HasErrorProperty); set => SetValue(HasErrorProperty, value); }

    public new void Focus() => _input.Focus();

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Só dígitos, cortado no tamanho: colar "123 456" ou "123-456" também funciona.
        var digits = new string((e.NewTextValue ?? string.Empty).Where(char.IsAsciiDigit).Take(Length).ToArray());
        if (digits != e.NewTextValue)
        {
            _input.Text = digits;
            return;
        }

        Code = digits;
    }

    private void OnCodeChanged()
    {
        if (_input.Text != Code) _input.Text = Code;
        HasError = false;
        Render();
        if (Code.Length == Length) Completed?.Invoke(this, Code);
    }

    private void BuildBoxes()
    {
        _boxes.Children.Clear();
        _input.MaxLength = Length * 2; // folga para colar com separadores
        for (var i = 0; i < Length; i++)
        {
            _boxes.Children.Add(new Border
            {
                WidthRequest = 48,
                HeightRequest = 56,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Content = new Label
                {
                    FontFamily = "JakartaSemiBold",
                    FontSize = 22,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                },
            });
        }

        Render();
    }

    private void Render()
    {
        var active = Math.Min(Code.Length, Length - 1);
        for (var i = 0; i < _boxes.Children.Count; i++)
        {
            var box = (Border)_boxes.Children[i];
            var label = (Label)box.Content!;
            label.Text = i < Code.Length ? Code[i].ToString() : string.Empty;
            label.SetThemeColor(Label.TextColorProperty, "Ink");
            box.SetThemeColor(BackgroundColorProperty, "Surface");

            // Erro em todas; senão a caixa atual ganha o anel de foco de 2px.
            var isCurrent = _input.IsFocused && i == active;
            box.SetThemeBrush(Border.StrokeProperty, HasError ? "Danger" : isCurrent ? "Accent" : "HairlineStrong");
            box.StrokeThickness = HasError || isCurrent ? 2 : 1;
        }
    }
}
// /snippet:control.otp
