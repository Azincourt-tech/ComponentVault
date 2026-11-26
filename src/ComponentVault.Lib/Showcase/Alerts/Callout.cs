using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Alerts;

public enum CalloutKind
{
    Info,
    Success,
    Warning,
    Danger,
}

// snippet:control.callout
/// <summary>
/// Aviso em linha: ícone + título + texto, ação opcional e botão de fechar com alvo de 44px.
/// A cor vem só do fundo suave e do ícone; o texto continua na tinta, com contraste alto.
/// </summary>
public class Callout : ContentView
{
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CalloutKind), typeof(Callout), CalloutKind.Info,
        propertyChanged: (b, _, _) => ((Callout)b).Refresh());

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(Callout), null,
        propertyChanged: (b, _, _) => ((Callout)b).Refresh());

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(Callout), null,
        propertyChanged: (b, _, _) => ((Callout)b).Refresh());

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(Callout), null,
        propertyChanged: (b, _, _) => ((Callout)b).Refresh());

    public static readonly BindableProperty IsDismissibleProperty = BindableProperty.Create(
        nameof(IsDismissible), typeof(bool), typeof(Callout), true,
        propertyChanged: (b, _, _) => ((Callout)b).Refresh());

    private readonly Border _frame = new() { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Padding = new Thickness(14, 12, 4, 12) };
    private readonly MauiIcon _icon = new() { IconSize = 20, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 1, 0, 0) };
    private readonly Label _title = new() { FontFamily = "JakartaSemiBold", FontSize = 15 };
    private readonly Label _message = new() { FontFamily = "JakartaRegular", FontSize = 14, LineHeight = 1.4 };
    // BorderWidth 0: sem isso o botão do Windows desenha um contorno retangular em volta do texto.
    private readonly Button _action = new() { Padding = new Thickness(0), FontFamily = "JakartaSemiBold", FontSize = 14, BorderWidth = 0, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Start, MinimumHeightRequest = 32 };
    private readonly Button _dismiss = new() { Text = "✕", FontSize = 14, Padding = new Thickness(0), WidthRequest = 44, HeightRequest = 44, BorderWidth = 0, CornerRadius = 22, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, -10, 0, 0) };

    public Callout()
    {
        _title.SetThemeColor(Label.TextColorProperty, "Ink");
        _message.SetThemeColor(Label.TextColorProperty, "InkSecondary");
        _dismiss.SetThemeColor(Button.TextColorProperty, "InkSecondary");
        SemanticProperties.SetDescription(_dismiss, "Fechar aviso");
        _dismiss.Clicked += async (_, _) => await DismissAsync();
        _action.Clicked += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);

        var text = new VerticalStackLayout { Spacing = 2, Children = { _title, _message, _action } };
        var grid = new Grid
        {
            ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
            ColumnSpacing = 12,
        };
        grid.Add(_icon, 0);
        grid.Add(text, 1);
        grid.Add(_dismiss, 2);
        _frame.Content = grid;
        Content = _frame;
        Refresh();
    }

    public event EventHandler? ActionClicked;

    public event EventHandler? Dismissed;

    public CalloutKind Kind { get => (CalloutKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public string? Message { get => (string?)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }

    public string? ActionText { get => (string?)GetValue(ActionTextProperty); set => SetValue(ActionTextProperty, value); }

    public bool IsDismissible { get => (bool)GetValue(IsDismissibleProperty); set => SetValue(IsDismissibleProperty, value); }

    /// <summary>Some em 150ms e libera o espaço; quem usa decide se mostra de novo.</summary>
    public async Task DismissAsync()
    {
        await this.FadeToAsync(0, 150, Easing.CubicOut);
        IsVisible = false;
        Opacity = 1;
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    private void Refresh()
    {
        var (icon, tint, accent) = Kind switch
        {
            CalloutKind.Success => (FluentIcons.CheckmarkCircle20, "SuccessTint", "SuccessText"),
            CalloutKind.Warning => (FluentIcons.Warning20, "WarningTint", "WarningText"),
            CalloutKind.Danger => (FluentIcons.ErrorCircle20, "DangerTint", "DangerText"),
            _ => (FluentIcons.Info20, "InfoTint", "AccentText"),
        };

        _icon.Icon = icon;
        _icon.SetThemeColor(MauiIcon.IconColorProperty, accent);
        _frame.SetThemeColor(BackgroundColorProperty, tint);
        _action.SetThemeColor(Button.TextColorProperty, accent);

        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _message.Text = Message;
        _message.IsVisible = !string.IsNullOrEmpty(Message);
        _action.Text = ActionText;
        _action.IsVisible = !string.IsNullOrEmpty(ActionText);
        _dismiss.IsVisible = IsDismissible;

        SemanticProperties.SetDescription(this, $"{Kind}: {Title}. {Message}");
    }
}
// /snippet:control.callout
