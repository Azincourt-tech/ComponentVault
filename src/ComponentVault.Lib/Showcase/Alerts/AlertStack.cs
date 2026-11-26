using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Alerts;

// snippet:control.alertstack
/// <summary>
/// Pilha de avisos, como as notificações agrupadas do iOS: recolhida, mostra o mais recente na
/// frente e a borda de até dois atrás, com profundidade; tocar abre a lista com animação.
/// Fechar um aviso reorganiza a pilha. Cada aviso é um <see cref="Callout"/> sobre um fundo
/// opaco, para os de trás não aparecerem através do tom suave do da frente.
/// </summary>
[ContentProperty(nameof(Items))]
public class AlertStack : ContentView
{
    private const double Peek = 12;
    private const double Gap = 10;
    private const double ScaleStep = 0.04;
    private const uint Length = 360;
    private const string HeightAnimation = "AlertStackHeight";

    private readonly Grid _cards = new() { IsClippedToBounds = false };
    private readonly Label _count = new() { FontFamily = "JakartaSemiBold", FontSize = 15, VerticalOptions = LayoutOptions.Center };
    private readonly Button _toggle = new()
    {
        FontFamily = "JakartaSemiBold",
        FontSize = 14,
        Padding = new Thickness(12, 0),
        HeightRequest = 36,
        BorderWidth = 0,
        BackgroundColor = Colors.Transparent,
        HorizontalOptions = LayoutOptions.End,
    };
    private readonly Label _empty = new() { FontFamily = "JakartaRegular", FontSize = 14, Text = "Tudo em dia: nenhum aviso.", IsVisible = false };
    private readonly Dictionary<Callout, Border> _wrappers = [];
    private bool _expanded;

    public AlertStack()
    {
        _count.SetThemeColor(Label.TextColorProperty, "Ink");
        _toggle.SetThemeColor(Button.TextColorProperty, "AccentText");
        _empty.SetThemeColor(Label.TextColorProperty, "InkTertiary");
        _toggle.Clicked += (_, _) => SetExpanded(!_expanded);

        var header = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        header.Add(_count, 0);
        header.Add(_toggle, 1);

        Content = new VerticalStackLayout { Spacing = 12, Children = { header, _cards, _empty } };
        Items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>Avisos, do mais recente (na frente) para o mais antigo.</summary>
    public ObservableCollection<Callout> Items { get; } = [];

    /// <summary>Mostra de novo os avisos fechados.</summary>
    public void Restore()
    {
        foreach (var item in Items) item.IsVisible = true;
        Relayout(animated: true);
    }

    private IReadOnlyList<Callout> Visible => Items.Where(i => i.IsVisible).ToList();

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems?.OfType<Callout>() ?? [])
        {
            var wrapper = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                VerticalOptions = LayoutOptions.Start,
                // Escala ancorada na base: cada aviso de trás aparece por inteiro (Peek) embaixo do da frente.
                AnchorY = 1,
                Content = item,
            };
            wrapper.SetThemeColor(BackgroundColorProperty, "Chrome");
            wrapper.SetThemeColor(Border.StrokeProperty, "ChromeStroke");

            // Recolhida, tocar no aviso da frente abre a pilha (os botões dele continuam valendo).
            wrapper.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { if (!_expanded) SetExpanded(true); }) });
            wrapper.SizeChanged += (_, _) => Dispatcher.Dispatch(() => Relayout(animated: false));
            item.Dismissed += (_, _) => Relayout(animated: true);
            item.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(IsVisible)) wrapper.IsVisible = item.IsVisible;
            };

            _wrappers[item] = wrapper;
            // O mais recente fica por cima: entra no fim da lista de filhos do Grid.
            _cards.Children.Insert(0, wrapper);
        }

        Relayout(animated: false);
    }

    private void SetExpanded(bool expanded)
    {
        _expanded = expanded;
        Relayout(animated: true);
    }

    private void Relayout(bool animated)
    {
        var visible = Visible;
        var count = visible.Count;
        _count.Text = count == 1 ? "1 aviso" : $"{count} avisos";
        _toggle.IsVisible = count > 1;
        _toggle.Text = _expanded ? "Recolher" : "Mostrar todos";
        _empty.IsVisible = count == 0;
        _cards.IsVisible = count > 0;
        if (count == 0 || _wrappers[visible[0]].Height <= 0) return;

        var offset = 0d;
        for (var i = 0; i < count; i++)
        {
            var card = _wrappers[visible[i]];
            var (y, scale, opacity) = _expanded
                ? (offset, 1d, 1d)
                : (i * Peek, 1 - (i * ScaleStep), i switch { 0 => 1d, 1 => 0.75, 2 => 0.45, _ => 0d });
            offset += card.Height + Gap;

            // Recolhida, só o da frente recebe toque; os de trás são só profundidade: a placa
            // aparece, o conteúdo não (senão o texto vaza pelo tom translúcido do da frente).
            card.InputTransparent = !_expanded && i > 0;
            card.ZIndex = count - i;
            Move(card, y, scale, opacity, animated);
            Fade(visible[i], _expanded || i == 0 ? 1 : 0, animated);
        }

        var front = _wrappers[visible[0]].Height;
        var height = _expanded ? offset - Gap : front + (Math.Min(count - 1, 2) * Peek);
        AnimateHeight(height, animated);
    }

    private static void Move(View card, double y, double scale, double opacity, bool animated)
    {
        if (!animated)
        {
            card.TranslationY = y;
            card.Scale = scale;
            card.Opacity = opacity;
            return;
        }

        _ = card.TranslateToAsync(0, y, Length, Easing.SpringOut);
        _ = card.ScaleToAsync(scale, Length, Easing.CubicOut);
        _ = card.FadeToAsync(opacity, Length, Easing.CubicOut);
    }

    private static void Fade(View content, double opacity, bool animated)
    {
        if (animated) _ = content.FadeToAsync(opacity, Length / 2, Easing.CubicOut);
        else content.Opacity = opacity;
    }

    private void AnimateHeight(double height, bool animated)
    {
        _cards.AbortAnimation(HeightAnimation);
        var from = _cards.HeightRequest > 0 ? _cards.HeightRequest : height;
        if (!animated || Math.Abs(from - height) < 0.5)
        {
            _cards.HeightRequest = height;
            return;
        }

        new Animation(v => _cards.HeightRequest = v, from, height, Easing.CubicOut)
            .Commit(_cards, HeightAnimation, rate: 16, length: Length);
    }
}
// /snippet:control.alertstack
