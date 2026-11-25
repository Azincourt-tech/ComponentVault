using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Toasts;

public enum ToastKind
{
    Neutral,
    Success,
    Error,
}

// snippet:control.toast
/// <summary>
/// Aviso temporário dentro da própria tela (não vira notificação do sistema no Windows).
/// Entra em 200ms subindo 16px, sai sozinho depois de ~3s e um aviso novo substitui o anterior.
/// Ação opcional (ex.: "Desfazer") para erros recuperáveis.
/// </summary>
public class InAppToast : ContentView
{
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(3);

    private readonly MauiIcon _icon = new() { IconSize = 20, VerticalOptions = LayoutOptions.Center };
    private readonly Label _message = new() { FontFamily = "JakartaMedium", FontSize = 15, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation };
    private readonly Button _action = new() { FontFamily = "JakartaSemiBold", FontSize = 15, Padding = new Thickness(12, 0), BackgroundColor = Colors.Transparent, MinimumHeightRequest = 40 };
    private CancellationTokenSource? _cts;
    private Action? _onAction;

    public InAppToast()
    {
        IsVisible = false;
        VerticalOptions = LayoutOptions.End;
        HorizontalOptions = LayoutOptions.Center;
        MaximumWidthRequest = 520;

        // Superfície invertida: tinta no fundo, texto na cor do fundo da página.
        var frame = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 6, 6, 6),
            MinimumHeightRequest = 52,
        };
        frame.SetThemeColor(BackgroundColorProperty, "Ink");
        _message.SetThemeColor(Label.TextColorProperty, "OnInk");
        // Cor da ação: o azul de destaque na versão para fundo escuro (e vice-versa).
        _action.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#7EA8F6"), Color.FromArgb("#2560CC"));
        _action.Clicked += async (_, _) =>
        {
            _onAction?.Invoke();
            await HideAsync();
        };

        var grid = new Grid
        {
            ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)],
            ColumnSpacing = 10,
        };
        grid.Add(_icon, 0);
        grid.Add(_message, 1);
        grid.Add(_action, 2);
        frame.Content = grid;
        Content = frame;

        Unloaded += (_, _) => _cts?.Cancel();
    }

    public async Task ShowAsync(string message, ToastKind kind = ToastKind.Neutral, string? actionText = null, Action? onAction = null, TimeSpan? duration = null)
    {
        _cts?.Cancel();
        var cts = _cts = new CancellationTokenSource();

        _message.Text = message;
        _onAction = onAction;
        _action.Text = actionText;
        _action.IsVisible = actionText is not null;
        _icon.IsVisible = kind != ToastKind.Neutral;
        if (kind == ToastKind.Success)
        {
            _icon.Icon = FluentIcons.CheckmarkCircle20;
            _icon.SetAppThemeColor(MauiIcon.IconColorProperty, Color.FromArgb("#4CC38A"), Color.FromArgb("#18704A"));
        }
        else if (kind == ToastKind.Error)
        {
            _icon.Icon = FluentIcons.ErrorCircle20;
            _icon.SetAppThemeColor(MauiIcon.IconColorProperty, Color.FromArgb("#F07A7A"), Color.FromArgb("#B53131"));
        }

        SemanticScreenReader.Default.Announce(message);

        this.AbortAnimation("FadeTo");
        this.AbortAnimation("TranslateTo");
        Opacity = 0;
        TranslationY = 16;
        IsVisible = true;
        await Task.WhenAll(
            this.FadeToAsync(1, 200, Easing.CubicOut),
            this.TranslateToAsync(0, 0, 200, Easing.CubicOut));

        try
        {
            await Task.Delay(duration ?? DefaultDuration, cts.Token);
        }
        catch (TaskCanceledException)
        {
            return; // Substituído por um aviso mais novo ou pela ação.
        }

        await HideAsync();
    }

    public async Task HideAsync()
    {
        _cts?.Cancel();
        await this.FadeToAsync(0, 150, Easing.CubicOut);
        IsVisible = false;
    }
}
// /snippet:control.toast
