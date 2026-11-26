namespace ComponentVault.Lib.Showcase.Cards;

// snippet:control.flipcard
/// <summary>
/// Card de duas faces que vira ao tocar: gira até ficar de lado (140ms), troca a face e completa o
/// giro (160ms). As duas faces ocupam o mesmo espaço, então o layout em volta não se mexe.
/// </summary>
[ContentProperty(nameof(Front))]
public class FlipCard : ContentView
{
    public static readonly BindableProperty FrontProperty = BindableProperty.Create(
        nameof(Front), typeof(View), typeof(FlipCard), propertyChanged: (b, _, _) => ((FlipCard)b).Build());

    public static readonly BindableProperty BackProperty = BindableProperty.Create(
        nameof(Back), typeof(View), typeof(FlipCard), propertyChanged: (b, _, _) => ((FlipCard)b).Build());

    public static readonly BindableProperty IsFlippedProperty = BindableProperty.Create(
        nameof(IsFlipped), typeof(bool), typeof(FlipCard), false, BindingMode.TwoWay);

    private readonly Grid _faces = new();
    private bool _busy;

    public FlipCard()
    {
        Content = _faces;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await FlipAsync();
        GestureRecognizers.Add(tap);
        SemanticProperties.SetHint(this, "Toque para virar");
    }

    public View? Front { get => (View?)GetValue(FrontProperty); set => SetValue(FrontProperty, value); }

    public View? Back { get => (View?)GetValue(BackProperty); set => SetValue(BackProperty, value); }

    public bool IsFlipped { get => (bool)GetValue(IsFlippedProperty); private set => SetValue(IsFlippedProperty, value); }

    public async Task FlipAsync()
    {
        if (_busy || Front is null || Back is null) return;
        _busy = true;
        var (hide, show) = IsFlipped ? (Back, Front) : (Front, Back);

        // Primeira metade: a face atual gira até ficar de lado (invisível).
        await _faces.RotateYToAsync(90, 140, Easing.CubicIn);
        hide.IsVisible = false;
        show.IsVisible = true;
        IsFlipped = !IsFlipped;

        // Segunda metade: a outra face entra do lado oposto e termina de frente.
        _faces.RotationY = -90;
        await _faces.RotateYToAsync(0, 160, Easing.CubicOut);
        SemanticScreenReader.Announce(IsFlipped ? "Verso" : "Frente");
        _busy = false;
    }

    private void Build()
    {
        _faces.Children.Clear();
        if (Front is not null) { Front.IsVisible = !IsFlipped; _faces.Children.Add(Front); }
        if (Back is not null) { Back.IsVisible = IsFlipped; _faces.Children.Add(Back); }
    }
}
// /snippet:control.flipcard
