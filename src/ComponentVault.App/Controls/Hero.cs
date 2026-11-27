namespace ComponentVault.App.Controls;

/// <summary>
/// Transição "o card vira o cabeçalho": ao tocar numa linha do catálogo, o ícone e o título dela
/// voam até o lugar que ocupam no cabeçalho do detalhe, enquanto o resto da página entra.
/// <para>
/// No XAML: a linha tocável recebe <c>Hero.Source="True"</c>; o ícone e o título, nas duas
/// páginas, recebem <c>Hero.Key="thumb"</c> / <c>Hero.Key="title"</c>. A página de destino chama
/// <see cref="PlayInto"/> quando carrega. Sem toque recente (link direto, voltar), nada acontece.
/// </para>
/// <para>
/// Na volta, o detalhe chama <see cref="CaptureReturn"/> ao sair e a página de origem chama
/// <see cref="PlayReturn"/> ao reaparecer: ícone e título descem do cabeçalho para a linha.
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

    // Para a volta: a linha tocada (e o item que ela mostrava) e onde o cabeçalho estava ao sair.
    private static WeakReference<Element>? s_sourceRow;
    private static object? s_sourceItem;
    private static Dictionary<string, Rect>? s_return;
    private static DateTime s_returnAt;

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
        s_sourceRow = new WeakReference<Element>(row);
        s_sourceItem = row.BindingContext;
    }

    /// <summary>Guarda onde os elementos marcados de <paramref name="header"/> estão, antes de a página sair.</summary>
    public static void CaptureReturn(Element header)
    {
        var bounds = new Dictionary<string, Rect>();
        foreach (var (view, key) in Keyed(header))
        {
            if (WindowBounds(view) is { } rect && rect.Width > 0) bounds[key] = rect;
        }

        s_return = bounds.Count > 0 ? bounds : null;
        s_returnAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Na página que reaparece: se a linha tocada é dela, ícone e título descem do cabeçalho
    /// guardado até a linha. Voam como cópias numa camada por cima de <paramref name="overlay"/>,
    /// porque a lista recorta o que passa fora dela; a linha real reaparece quando a cópia pousa.
    /// </summary>
    public static void PlayReturn(Grid overlay)
    {
        var from = s_return;
        s_return = null;
        if (from is null || DateTime.UtcNow - s_returnAt > MaxAge) return;
        if (s_sourceRow is null || !s_sourceRow.TryGetTarget(out var row)) return;

        // A linha precisa estar nesta página e ainda mostrar o mesmo item (a lista recicla linhas).
        if (!ReferenceEquals(row.BindingContext, s_sourceItem) || !IsInside(row, overlay)) return;

        var targets = Keyed(row).Where(t => from.ContainsKey(t.Key)).ToList();
        if (targets.Count == 0) return;

        foreach (var (view, _) in targets) view.Opacity = 0;
        WhenMeasured(targets, attempt: 0, () =>
        {
            var host = WindowBounds(overlay);
            foreach (var (view, key) in targets)
            {
                if (host is null || WindowBounds(view) is not { } to || to.Height <= 0 || Ghost(view) is not { } ghost)
                {
                    view.Opacity = 1;
                    continue;
                }

                FlyGhost(overlay, host.Value, ghost, from[key], to, view);
            }
        });
    }

    private static void FlyGhost(Grid overlay, Rect host, View ghost, Rect from, Rect to, VisualElement real)
    {
        ghost.WidthRequest = to.Width;
        ghost.HeightRequest = to.Height;
        ghost.HorizontalOptions = LayoutOptions.Start;
        ghost.VerticalOptions = LayoutOptions.Start;
        ghost.InputTransparent = true;
        ghost.AnchorX = 0;
        ghost.AnchorY = 0;
        // A cópia nasce no canto da área de conteúdo, que começa depois do padding do Grid.
        var originX = host.X + overlay.Padding.Left;
        var originY = host.Y + overlay.Padding.Top;
        ghost.TranslationX = from.X - originX;
        ghost.TranslationY = from.Y - originY;
        ghost.Scale = from.Height / to.Height;
        Grid.SetRowSpan(ghost, Math.Max(1, overlay.RowDefinitions.Count));
        Grid.SetColumnSpan(ghost, Math.Max(1, overlay.ColumnDefinitions.Count));
        overlay.Add(ghost);

        var flight = new Animation
        {
            { 0, 1, new Animation(v => ghost.TranslationX = v, ghost.TranslationX, to.X - originX, Easing.CubicInOut) },
            { 0, 1, new Animation(v => ghost.TranslationY = v, ghost.TranslationY, to.Y - originY, Easing.CubicInOut) },
            { 0, 1, new Animation(v => ghost.Scale = v, ghost.Scale, 1, Easing.CubicInOut) },
        };
        flight.Commit(ghost, "HeroReturn", rate: 16, length: Length, finished: (_, _) =>
        {
            real.Opacity = 1;
            overlay.Remove(ghost);
        });
    }

    // Cópia visual do elemento que voa: só o que a vitrine marca (ícone e título).
    private static View? Ghost(VisualElement view) => view switch
    {
        ComponentThumb thumb => new ComponentThumb { IconName = thumb.IconName, IconSize = thumb.IconSize },
        Label label => new Label
        {
            Text = label.Text,
            FontFamily = label.FontFamily,
            FontSize = label.FontSize,
            TextColor = label.TextColor,
            LineBreakMode = LineBreakMode.NoWrap,
        },
        _ => null,
    };

    private static bool IsInside(Element element, Element ancestor)
    {
        for (var current = element; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, ancestor)) return true;
        }
        return false;
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
