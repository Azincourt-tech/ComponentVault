using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace ComponentVault.Lib.Controls;

public enum FieldState
{
    Normal,
    Success,
    Error,
}

/// <summary>Campo de texto do design system: rótulo acima, ícone, botão de limpar, dica e estados de validação.</summary>
public partial class VaultTextField : ContentView
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(VaultTextField));

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(VaultTextField), null, BindingMode.TwoWay,
        propertyChanged: (b, o, n) => ((VaultTextField)b).OnTextChanged((string?)o, (string?)n));

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(VaultTextField));

    public static readonly BindableProperty HintProperty = BindableProperty.Create(
        nameof(Hint), typeof(string), typeof(VaultTextField));

    public static readonly BindableProperty IconDataProperty = BindableProperty.Create(
        nameof(IconData), typeof(Geometry), typeof(VaultTextField));

    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
        nameof(Keyboard), typeof(Keyboard), typeof(VaultTextField), Keyboard.Default);

    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
        nameof(IsPassword), typeof(bool), typeof(VaultTextField), false);

    public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(
        nameof(ReturnType), typeof(ReturnType), typeof(VaultTextField), ReturnType.Default);

    public static readonly BindableProperty ShowClearButtonProperty = BindableProperty.Create(
        nameof(ShowClearButton), typeof(bool), typeof(VaultTextField), true,
        propertyChanged: (b, _, _) => ((VaultTextField)b).UpdateClearButton());

    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State), typeof(FieldState), typeof(VaultTextField), FieldState.Normal,
        propertyChanged: (b, _, _) => ((VaultTextField)b).UpdateVisuals());

    public static readonly BindableProperty RefusesInitialFocusProperty = BindableProperty.Create(
        nameof(RefusesInitialFocus), typeof(bool), typeof(VaultTextField), false);

    // Janela em que um foco recebido é tratado como automático (dado pela plataforma), não pela pessoa.
    private static readonly TimeSpan InitialFocusWindow = TimeSpan.FromSeconds(1);
    private DateTime _loadedAt = DateTime.MinValue;

    public VaultTextField()
    {
        InitializeComponent();
        UpdateVisuals();
        Loaded += (_, _) => _loadedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recusa o foco que a plataforma dá sozinha ao abrir a página (o Windows foca o primeiro
    /// campo; no celular isso abriria o teclado sem pedir). O toque da pessoa continua focando.
    /// </summary>
    public bool RefusesInitialFocus
    {
        get => (bool)GetValue(RefusesInitialFocusProperty);
        set => SetValue(RefusesInitialFocusProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    public string? Placeholder { get => (string?)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    public string? Hint { get => (string?)GetValue(HintProperty); set => SetValue(HintProperty, value); }

    /// <summary>Ícone à esquerda como geometria de Path (ex.: "M7 1a6 6 0 1 0 0 12...").</summary>
    [System.ComponentModel.TypeConverter(typeof(PathGeometryConverter))]
    public Geometry? IconData { get => (Geometry?)GetValue(IconDataProperty); set => SetValue(IconDataProperty, value); }

    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }

    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }

    public ReturnType ReturnType { get => (ReturnType)GetValue(ReturnTypeProperty); set => SetValue(ReturnTypeProperty, value); }

    public bool ShowClearButton { get => (bool)GetValue(ShowClearButtonProperty); set => SetValue(ShowClearButtonProperty, value); }

    public FieldState State { get => (FieldState)GetValue(StateProperty); set => SetValue(StateProperty, value); }

    public new void Focus() => Input.Focus();

    private void OnTextChanged(string? oldValue, string? newValue)
    {
        UpdateClearButton();
        TextChanged?.Invoke(this, new TextChangedEventArgs(oldValue, newValue));
    }

    private void OnClearTapped(object? sender, TappedEventArgs e)
    {
        Text = string.Empty;
        Input.Focus();
    }

    private void OnInputFocusChanged(object? sender, FocusEventArgs e)
    {
        if (e.IsFocused && RefusesInitialFocus && DateTime.UtcNow - _loadedAt < InitialFocusWindow)
        {
            Dispatcher.Dispatch(() => Input.Unfocus());
            return;
        }

        UpdateVisuals();
    }

    private void UpdateClearButton() =>
        ClearButton.IsVisible = ShowClearButton && !IsPassword && !string.IsNullOrEmpty(Text);

    private void UpdateVisuals()
    {
        // Erro e sucesso vencem o foco, para o feedback de validação não sumir ao digitar.
        var (borderKey, hintKey) = State switch
        {
            FieldState.Error => ("Danger", "DangerText"),
            FieldState.Success => ("Success", "SuccessText"),
            _ when Input.IsFocused => ("Accent", "InkTertiary"),
            _ => ("HairlineStrong", "InkTertiary"),
        };

        FieldBorder.SetThemeBrush(Border.StrokeProperty, borderKey);
        HintLabel.SetThemeColor(MauiLabel.TextColorProperty, hintKey);
        // Foco: anel de 2px na cor de destaque, sem animação.
        FieldBorder.StrokeThickness = Input.IsFocused || State != FieldState.Normal ? 2 : 1;
    }
}
