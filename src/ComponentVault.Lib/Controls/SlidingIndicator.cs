namespace ComponentVault.Lib.Controls;

/// <summary>
/// Move um indicador (pílula, sublinhado) até o item selecionado de uma linha, com deslize
/// "líquido": a posição passa um pouco do ponto e volta, e a largura acompanha sem quicar.
/// O indicador fica na mesma célula da linha, alinhado à esquerda, e anda por TranslationX.
/// </summary>
public sealed class SlidingIndicator
{
    private const string SlideName = "IndicatorSlide";

    private readonly View _indicator;
    private (double X, double Width)? _goal;

    public SlidingIndicator(View indicator, Layout row)
    {
        _indicator = indicator;
        indicator.HorizontalOptions = LayoutOptions.Start;
        indicator.InputTransparent = true;

        // O SizeChanged da linha dispara ANTES de os itens serem posicionados, e um item que só
        // muda de X (porque o vizinho cresceu) nem dispara o seu. Por isso a medição é adiada
        // para depois da passada de layout: ler X/Width ali dava medidas velhas e o indicador
        // ficava torto.
        row.SizeChanged += (_, _) => Remeasure();
        foreach (var child in row.Children.OfType<VisualElement>()) child.SizeChanged += (_, _) => Remeasure();

        // Itens que chegam depois (BindableLayout preenchido quando os dados carregam).
        row.ChildAdded += (_, e) =>
        {
            if (e.Element is VisualElement child) child.SizeChanged += (_, _) => Remeasure();
            Remeasure();
        };
        row.ChildRemoved += (_, _) => Remeasure();
    }

    /// <summary>Item que o indicador deve marcar. Mudar o alvo desliza até ele.</summary>
    public View? Target { get; private set; }

    /// <summary>Disparado quando o layout fica pronto (ou muda) e o alvo pode ser medido.</summary>
    public event EventHandler? Measured;

    public void SlideTo(View? target)
    {
        Target = target;
        if (target is null || target.Width <= 0) return;
        if (_goal is null)
        {
            JumpTo(target);
            return;
        }

        var fromX = _indicator.TranslationX;
        var fromWidth = _indicator.WidthRequest > 0 ? _indicator.WidthRequest : target.Width;
        var toX = target.X;
        var toWidth = target.Width;
        _goal = (toX, toWidth);

        _indicator.AbortAnimation(SlideName);
        new Animation(t =>
        {
            _indicator.TranslationX = fromX + ((toX - fromX) * Easing.SpringOut.Ease(t));
            _indicator.WidthRequest = fromWidth + ((toWidth - fromWidth) * Easing.CubicOut.Ease(t));
        }).Commit(_indicator, SlideName, rate: 16, length: 380, easing: Easing.Linear);
    }

    public void JumpTo(View? target)
    {
        Target = target;
        if (target is null || target.Width <= 0) return;
        _indicator.AbortAnimation(SlideName);
        _indicator.TranslationX = target.X;
        _indicator.WidthRequest = target.Width;
        _goal = (target.X, target.Width);
    }

    private void Remeasure() => _indicator.Dispatcher.Dispatch(() =>
    {
        Measured?.Invoke(this, EventArgs.Empty);

        var target = Target;
        if (target is null || target.Width <= 0) return;

        // Parado: encaixa direto. Deslizando: só refaz o deslize se o destino mudou de verdade.
        if (!_indicator.AnimationIsRunning(SlideName))
        {
            JumpTo(target);
        }
        else if (_goal is not { } goal || Math.Abs(goal.X - target.X) > 0.5 || Math.Abs(goal.Width - target.Width) > 0.5)
        {
            SlideTo(target);
        }
    });
}
