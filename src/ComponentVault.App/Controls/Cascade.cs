namespace ComponentVault.App.Controls;

/// <summary>
/// Entrada em cascata para itens de lista: cada item sobe 16px e aparece, um pouco depois do
/// anterior. Só anima o que surge logo após <see cref="Begin"/> (abrir a página, trocar o filtro);
/// o que aparece depois, ao rolar ou digitar na busca, entra parado. Animar a cada tecla ou a
/// cada item rolado distrai e pesa na lista virtualizada.
/// <para>No XAML: <c>controls:Cascade.Item="True"</c> na raiz do template do item.</para>
/// </summary>
public static class Cascade
{
    public static readonly BindableProperty ItemProperty = BindableProperty.CreateAttached(
        "Item", typeof(bool), typeof(Cascade), false, propertyChanged: OnItemChanged);

    // Em qual abertura o item já animou: um contêiner reciclado não repete a entrada.
    private static readonly BindableProperty GenerationProperty = BindableProperty.CreateAttached(
        "Generation", typeof(int), typeof(Cascade), 0);

    private const string AnimationName = "CascadeEntrance";
    private const int StaggerMs = 40;
    private const int MaxSteps = 12;
    private const uint Length = 380;
    private static readonly TimeSpan Window = TimeSpan.FromMilliseconds(700);

    private static int s_generation;
    private static DateTime s_openedAt = DateTime.MinValue;
    private static int s_count;

    public static bool GetItem(BindableObject view) => (bool)view.GetValue(ItemProperty);

    public static void SetItem(BindableObject view, bool value) => view.SetValue(ItemProperty, value);

    /// <summary>Abre uma nova janela de cascata: os próximos itens a aparecer entram em sequência.</summary>
    public static void Begin()
    {
        s_generation++;
        s_openedAt = DateTime.UtcNow;
        s_count = 0;
    }

    private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view || newValue is not true) return;

        // Loaded cobre o item novo; BindingContextChanged, o contêiner reaproveitado com outro item.
        view.Loaded += (_, _) => TryPlay(view);
        view.BindingContextChanged += (_, _) => TryPlay(view);
    }

    private static void TryPlay(VisualElement view)
    {
        if (DateTime.UtcNow - s_openedAt > Window) return;
        if ((int)view.GetValue(GenerationProperty) == s_generation) return;
        var generation = s_generation;
        view.SetValue(GenerationProperty, generation);

        var step = Math.Min(s_count++, MaxSteps);
        view.AbortAnimation(AnimationName);
        view.Opacity = 0;
        view.TranslationY = 16;

        var entrance = new Animation
        {
            { 0, 1, new Animation(v => view.Opacity = v, 0, 1, Easing.CubicOut) },
            { 0, 1, new Animation(v => view.TranslationY = v, 16, 0, Easing.CubicOut) },
        };

        // Termina sempre visível, mesmo que a animação seja interrompida no meio. Um atraso que
        // vence depois de uma abertura mais nova não mexe mais no item.
        view.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(step * StaggerMs), () =>
        {
            if ((int)view.GetValue(GenerationProperty) != generation) return;
            entrance.Commit(view, AnimationName, rate: 16, length: Length,
                finished: (_, _) =>
                {
                    view.Opacity = 1;
                    view.TranslationY = 0;
                });
        });
    }
}
