using System.Collections.ObjectModel;

namespace ComponentVault.Lib.Showcase.Carousels;

// snippet:control.cardstack
/// <summary>
/// Pilha de cartões: o de cima fica inteiro e os de trás aparecem um pouco abaixo e menores, como um
/// baralho. Arrastar o de cima para o lado (ou chamar <see cref="Next"/>) o faz voar e ir para o fim
/// da pilha; soltar antes do limite o devolve ao lugar. <see cref="Previous"/> traz o último de volta.
/// </summary>
[ContentProperty(nameof(Cards))]
public class CardStack : ContentView
{
    private const double Peek = 12;          // quanto cada cartão de trás aparece embaixo
    private const double ShrinkPerLevel = 0.05;
    private const int VisibleBehind = 2;
    private const double SwipeThreshold = 90;

    private readonly Grid _host = new();
    private readonly List<View> _order = []; // _order[0] é o de cima
    private bool _busy;

    public event EventHandler? CurrentChanged;

    public CardStack()
    {
        Content = _host;
        // Espaço embaixo para os cartões de trás, que descem além da altura do de cima.
        Padding = new Thickness(0, 0, 0, Peek * VisibleBehind);
        Cards.CollectionChanged += (_, _) => Rebuild();
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(pan);
    }

    public ObservableCollection<View> Cards { get; } = [];

    /// <summary>Posição (0 a n-1) do cartão de cima na ordem original.</summary>
    public int CurrentIndex => _order.Count == 0 ? -1 : Cards.IndexOf(_order[0]);

    public async void Next() => await FlyOutAsync(-1);

    public async void Previous()
    {
        if (_busy || _order.Count < 2) return;
        _busy = true;
        // O último volta para o topo entrando pelo lado esquerdo.
        var card = _order[^1];
        _order.RemoveAt(_order.Count - 1);
        _order.Insert(0, card);
        card.TranslationX = -Width;
        card.Rotation = -8;
        Arrange(animated: true, except: card);
        await Task.WhenAll(card.TranslateToAsync(0, 0, 220, Easing.CubicOut), card.RotateToAsync(0, 220, Easing.CubicOut));
        _busy = false;
        Changed();
    }

    private void Rebuild()
    {
        _host.Children.Clear();
        _order.Clear();
        _order.AddRange(Cards);
        foreach (var card in Cards) _host.Children.Add(card);
        Arrange(animated: false);
        Changed();
    }

    // Z-order: no Grid, o último filho fica por cima, então a lista do Grid é a ordem invertida.
    private void Arrange(bool animated, View? except = null)
    {
        for (var level = _order.Count - 1; level >= 0; level--)
        {
            var card = _order[level];
            _host.Children.Remove(card);
            _host.Children.Add(card);
        }

        for (var level = 0; level < _order.Count; level++)
        {
            var card = _order[level];
            if (card == except) continue;
            var depth = Math.Min(level, VisibleBehind + 1);
            var y = Peek * depth;
            var scale = 1 - ShrinkPerLevel * depth;
            var opacity = level <= VisibleBehind ? 1 : 0;
            card.InputTransparent = level != 0;
            if (animated)
            {
                _ = card.TranslateToAsync(0, y, 200, Easing.CubicOut);
                _ = card.ScaleToAsync(scale, 200, Easing.CubicOut);
                _ = card.FadeToAsync(opacity, 200);
            }
            else
            {
                card.TranslationX = 0;
                card.TranslationY = y;
                card.Scale = scale;
                card.Opacity = opacity;
            }
        }
    }

    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_busy || _order.Count == 0) return;
        var top = _order[0];
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                // O cartão segue o dedo e inclina na direção do arrasto.
                top.TranslationX = e.TotalX;
                top.Rotation = e.TotalX / 24;
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (Math.Abs(top.TranslationX) > SwipeThreshold)
                {
                    await FlyOutAsync(Math.Sign(top.TranslationX));
                }
                else
                {
                    await Task.WhenAll(top.TranslateToAsync(0, 0, 180, Easing.CubicOut), top.RotateToAsync(0, 180, Easing.CubicOut));
                }
                break;
        }
    }

    // Voa para o lado (direction -1 esquerda, +1 direita) e vai para o fim da pilha.
    private async Task FlyOutAsync(int direction)
    {
        if (_busy || _order.Count < 2) return;
        _busy = true;
        var top = _order[0];
        var distance = Math.Max(Width, 320) * 1.2 * direction;
        await Task.WhenAll(
            top.TranslateToAsync(distance, top.TranslationY, 200, Easing.CubicIn),
            top.RotateToAsync(14 * direction, 200, Easing.CubicIn));

        _order.RemoveAt(0);
        _order.Add(top);
        top.Rotation = 0;
        top.TranslationX = 0;
        top.Opacity = 0;
        Arrange(animated: true);
        _busy = false;
        Changed();
    }

    private void Changed()
    {
        if (_order.Count > 0) SemanticProperties.SetDescription(this, $"Cartão {CurrentIndex + 1} de {_order.Count}");
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}
// /snippet:control.cardstack
