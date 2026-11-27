using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Fabs;

/// <summary>Uma ação do <see cref="SpeedDial"/>: ícone e rótulo curto.</summary>
public sealed class SpeedDialAction
{
    public FluentIcons Icon { get; set; }

    public string Label { get; set; } = "";
}

// snippet:control.speeddial
/// <summary>
/// Botão flutuante que abre as ações em leque: o "+" gira 45° e vira "×", o fundo escurece e os
/// botões menores saem do centro do FAB num quarto de círculo (para cima e para a esquerda), um
/// depois do outro (40ms), com mola. Tocar numa ação ou fora fecha na ordem inversa. Fica no
/// canto de baixo à direita do container onde é colocado (mesma célula do Grid).
/// </summary>
public class SpeedDial : Grid
{
    private const double FabSize = 56;
    private const double MiniSize = 48;
    private const double Edge = 20;       // distância do FAB até as bordas
    private const double Radius = 112;    // raio do leque

    private readonly BoxView _scrim;
    private readonly Border _fab;
    private readonly MauiIcon _plus;
    private readonly List<View> _items = [];
    private IList<SpeedDialAction> _actions = [];
    private bool _open;

    public event EventHandler<SpeedDialAction>? ActionInvoked;

    public SpeedDial()
    {
        _scrim = new BoxView { Color = Colors.Black, Opacity = 0, IsVisible = false };
        var scrimTap = new TapGestureRecognizer();
        scrimTap.Tapped += (_, _) => Close();
        _scrim.GestureRecognizers.Add(scrimTap);
        Children.Add(_scrim);

        _plus = new MauiIcon { Icon = FluentIcons.Add24, IconSize = 26, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _plus.SetThemeColor(MauiIcon.IconColorProperty, "OnAccent");
        _fab = new Border
        {
            WidthRequest = FabSize,
            HeightRequest = FabSize,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, Edge, Edge),
            Content = _plus,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Opacity = 0.25f, Radius = 16, Offset = new Point(0, 6) },
        };
        _fab.SetThemeColor(BackgroundColorProperty, "Accent");
        SemanticProperties.SetDescription(_fab, "Criar");
        SemanticProperties.SetHint(_fab, "Abre as opções de criação");
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (_open) Close(); else Open(); };
        _fab.GestureRecognizers.Add(tap);
        Children.Add(_fab);
    }

    /// <summary>Ações do leque, da primeira (à esquerda) à última (em cima). Funciona bem com 2 a 4.</summary>
    public IList<SpeedDialAction> Actions
    {
        get => _actions;
        set
        {
            _actions = value;
            foreach (var old in _items) Children.Remove(old);
            _items.Clear();
            foreach (var action in value) _items.Add(Item(action));
            // Os itens ficam abaixo do FAB na pilha, para saírem "de dentro" dele.
            for (var i = 0; i < _items.Count; i++) Children.Insert(1 + i, _items[i]);
        }
    }

    public bool IsOpen => _open;

    // Botão menor com o rótulo embaixo, centrado no mesmo ponto que o FAB enquanto fechado.
    private View Item(SpeedDialAction action)
    {
        var icon = new MauiIcon { Icon = action.Icon, IconSize = 20, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        icon.SetThemeColor(MauiIcon.IconColorProperty, "Ink");
        var button = new Border
        {
            WidthRequest = MiniSize,
            HeightRequest = MiniSize,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HorizontalOptions = LayoutOptions.Center,
            Content = icon,
        };
        button.SetThemeColor(BackgroundColorProperty, "SurfaceRaised");
        button.SetThemeBrush(Border.StrokeProperty, "Hairline");

        var label = new Label
        {
            Text = action.Label,
            FontFamily = "JakartaSemiBold",
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.White,
        };

        const double width = 96;
        var item = new VerticalStackLayout
        {
            Spacing = 6,
            WidthRequest = width,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            // Centro do botão = centro do FAB: margem direita e de baixo compensam a largura do item.
            Margin = new Thickness(0, 0, Edge + (FabSize / 2) - (width / 2), Edge + (FabSize / 2) - (MiniSize / 2) - 24),
            Opacity = 0,
            Scale = 0.4,
            IsVisible = false,
            Children = { button, label },
        };
        SemanticProperties.SetDescription(item, action.Label);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            ActionInvoked?.Invoke(this, action);
            Close();
        };
        item.GestureRecognizers.Add(tap);
        return item;
    }

    // Ângulos de 180° (esquerda) a 90° (para cima), divididos igualmente entre as ações.
    private Point Target(int index)
    {
        var count = Math.Max(1, _items.Count);
        var angle = Math.PI - (count == 1 ? Math.PI / 4 : index * (Math.PI / 2) / (count - 1));
        return new Point(Math.Cos(angle) * Radius, -Math.Sin(angle) * Radius);
    }

    public async void Open()
    {
        if (_open) return;
        _open = true;
        SemanticProperties.SetHint(_fab, "Fecha as opções de criação");
        _scrim.IsVisible = true;
        _ = _scrim.FadeToAsync(0.45, 200);
        _ = _plus.RotateToAsync(45, 220, Easing.CubicOut);

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var to = Target(i);
            item.TranslationX = 0;
            item.TranslationY = 0;
            item.IsVisible = true;
            _ = Task.WhenAll(
                item.FadeToAsync(1, 160),
                item.ScaleToAsync(1, 320, Easing.SpringOut),
                item.TranslateToAsync(to.X, to.Y, 320, Easing.SpringOut));
            await Task.Delay(40);
        }
    }

    public async void Close()
    {
        if (!_open) return;
        _open = false;
        SemanticProperties.SetHint(_fab, "Abre as opções de criação");
        _ = _scrim.FadeToAsync(0, 160);
        _ = _plus.RotateToAsync(0, 200, Easing.CubicOut);

        var closing = _items.AsEnumerable().Reverse().Select(async (item, i) =>
        {
            await Task.Delay(i * 30);
            await Task.WhenAll(
                item.FadeToAsync(0, 140),
                item.ScaleToAsync(0.4, 160, Easing.CubicIn),
                item.TranslateToAsync(0, 0, 160, Easing.CubicIn));
            if (!_open) item.IsVisible = false;
        }).ToList();
        await Task.WhenAll(closing);
        if (!_open) _scrim.IsVisible = false;
    }
}
// /snippet:control.speeddial
