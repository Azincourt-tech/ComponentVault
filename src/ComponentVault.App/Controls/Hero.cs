namespace ComponentVault.App.Controls;

/// <summary>
/// Transição "o card vira o cabeçalho": ao tocar numa linha do catálogo, o ícone e o título dela
/// voam até o lugar que ocupam no cabeçalho do detalhe, enquanto o resto da página entra.
/// <para>
/// No XAML: a linha tocável recebe <c>Hero.Source="True"</c>; o ícone e o título, nas duas
/// páginas, recebem <c>Hero.Key="thumb"</c> / <c>Hero.Key="title"</c>. A página de destino chama
/// <see cref="PlayInto"/> quando carrega. Sem toque recente (link direto, voltar), nada acontece.
/// </para>
/// </summary>
public static class Hero
{
    public static readonly BindableProperty KeyProperty = BindableProperty.CreateAttached(
        "Key", typeof(string), typeof(Hero), null);

    public static readonly BindableProperty SourceProperty = BindableProperty.CreateAttached(
        "Source", typeof(bool), typeof(Hero), false, propertyChanged: OnSourceChanged);

    // A origem só vale logo depois do toque: a navegação leva algumas centenas de ms.
    private static readonly TimeSpan MaxAge = TimeSpan.FromSeconds(2);
    private const uint Length = 460;

    private static Dictionary<string, Rect>? s_origin;
    private static DateTime s_capturedAt;

    public static string? GetKey(BindableObject view) => (string?)view.GetValue(KeyProperty);

    public static void SetKey(BindableObject view, string? value) => view.SetValue(KeyProperty, value);

    public static bool GetSource(BindableObject view) => (bool)view.GetValue(SourceProperty);

    public static void SetSource(BindableObject view, bool value) => view.SetValue(SourceProperty, value);

    /// <summary>Faz os elementos marcados de <paramref name="root"/> saírem da posição guardada no toque.</summary>
    public static void PlayInto(Element root)
    {
        var origin = s_origin;
        s_origin = null;
        if (origin is null || DateTime.UtcNow - s_capturedAt > MaxAge) return;

        var targets = Keyed(root)
            .Where(t => origin.ContainsKey(t.Key))
            .ToList();
        if (targets.Count == 0) return;

        // Escondidos até a primeira medição, para não piscar no lugar final antes de voar.
        foreach (var (view, _) in targets) view.Opacity = 0;
        WhenMeasured(targets, attempt: 0, () =>
        {
            foreach (var (view, key) in targets) Fly(view, origin[key]);
        });
    }

    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View row || newValue is not true) return;

        // Um reconhecedor a mais, só para guardar a origem: o comando da linha continua o mesmo.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Capture(row);
        row.GestureRecognizers.Add(tap);
    }

    private static void Capture(Element row)
    {
        var origin = new Dictionary<string, Rect>();
        foreach (var (view, key) in Keyed(row))
        {
            if (WindowBounds(view) is { } bounds) origin[key] = bounds;
        }

        s_origin = origin.Count > 0 ? origin : null;
        s_capturedAt = DateTime.UtcNow;
    }

    private static void Fly(VisualElement view, Rect from)
    {
        if (WindowBounds(view) is not { } to || to.Width <= 0 || to.Height <= 0)
        {
            view.Opacity = 1;
            return;
        }

        // Escala pela altura: no título o Label ocupa a largura toda, mas o texto cresce na altura.
        var scale = from.Height / to.Height;
        view.AnchorX = 0;
        view.AnchorY = 0;
        view.TranslationX = from.X - to.X;
        view.TranslationY = from.Y - to.Y;
        view.Scale = scale;
        view.Opacity = 1;

        _ = view.TranslateToAsync(0, 0, Length, Easing.CubicInOut);
        _ = view.ScaleToAsync(1, Length, Easing.CubicInOut);
    }

    // A página nova ainda não foi medida quando carrega: tenta de novo a cada quadro, por pouco tempo.
    private static void WhenMeasured(List<(VisualElement View, string Key)> targets, int attempt, Action action)
    {
        if (targets.All(t => t.View.Width > 0 && WindowBounds(t.View) is not null) || attempt > 30)
        {
            action();
            return;
        }

        targets[0].View.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(16),
            () => WhenMeasured(targets, attempt + 1, action));
    }

    private static IEnumerable<(VisualElement View, string Key)> Keyed(Element root)
    {
        if (root is VisualElement view && GetKey(view) is { } key) yield return (view, key);
        if (root is not IVisualTreeElement tree) yield break;

        foreach (var child in tree.GetVisualChildren().OfType<Element>())
        {
            foreach (var found in Keyed(child)) yield return found;
        }
    }

    /// <summary>Retângulo do elemento em coordenadas da janela (unidades independentes de densidade).</summary>
    private static Rect? WindowBounds(VisualElement view)
    {
#if WINDOWS
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element && element.XamlRoot is not null)
        {
            var point = element.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
            return new Rect(point.X, point.Y, view.Width, view.Height);
        }
#elif ANDROID
        if (view.Handler?.PlatformView is Android.Views.View element && element.IsAttachedToWindow)
        {
            var location = new int[2];
            element.GetLocationInWindow(location);
            var density = DeviceDisplay.MainDisplayInfo.Density;
            return new Rect(location[0] / density, location[1] / density, view.Width, view.Height);
        }
#endif
        return null;
    }
}
