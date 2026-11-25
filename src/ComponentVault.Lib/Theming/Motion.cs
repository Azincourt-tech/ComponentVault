namespace ComponentVault.Lib.Theming;

// snippet:control.motion
/// <summary>
/// Movimento do design system como propriedades anexadas, para usar direto no XAML:
/// <code>
///   &lt;Border theming:Motion.Entrance="True" theming:Motion.Delay="120" /&gt;
///   &lt;Border theming:Motion.Press="True" /&gt;
/// </code>
/// Entrada: sobe 14px e aparece em 520ms (CubicOut), uma vez por elemento.
/// Toque: encolhe para 97,5% em 90ms e volta em 160ms. Sem bounce em nenhum dos dois.
/// </summary>
public static class Motion
{
    public static readonly BindableProperty EntranceProperty = BindableProperty.CreateAttached(
        "Entrance", typeof(bool), typeof(Motion), false, propertyChanged: OnEntranceChanged);

    public static readonly BindableProperty DelayProperty = BindableProperty.CreateAttached(
        "Delay", typeof(int), typeof(Motion), 0);

    public static readonly BindableProperty PressProperty = BindableProperty.CreateAttached(
        "Press", typeof(bool), typeof(Motion), false, propertyChanged: OnPressChanged);

    public static bool GetEntrance(BindableObject view) => (bool)view.GetValue(EntranceProperty);

    public static void SetEntrance(BindableObject view, bool value) => view.SetValue(EntranceProperty, value);

    public static int GetDelay(BindableObject view) => (int)view.GetValue(DelayProperty);

    public static void SetDelay(BindableObject view, int value) => view.SetValue(DelayProperty, value);

    public static bool GetPress(BindableObject view) => (bool)view.GetValue(PressProperty);

    public static void SetPress(BindableObject view, bool value) => view.SetValue(PressProperty, value);

    private static void OnEntranceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view || newValue is not true) return;

        view.Opacity = 0;
        view.TranslationY = 14;
        view.Loaded += OnLoaded;

        async void OnLoaded(object? sender, EventArgs e)
        {
            view.Loaded -= OnLoaded;
            var delay = GetDelay(view);
            if (delay > 0) await Task.Delay(delay);
            await Task.WhenAll(
                view.FadeToAsync(1, 520, Easing.CubicOut),
                view.TranslateToAsync(0, 0, 520, Easing.CubicOut));
        }
    }

    private static void OnPressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view || newValue is not true) return;

        var pointer = new PointerGestureRecognizer();
        pointer.PointerPressed += (_, _) => _ = view.ScaleToAsync(0.975, 90, Easing.CubicOut);
        pointer.PointerReleased += (_, _) => _ = view.ScaleToAsync(1, 160, Easing.CubicOut);
        pointer.PointerExited += (_, _) => _ = view.ScaleToAsync(1, 160, Easing.CubicOut);
        view.GestureRecognizers.Add(pointer);
    }
}
// /snippet:control.motion
