namespace ComponentVault.Lib.Theming;

/// <summary>
/// Largura "até N, sem passar do espaço disponível", alinhada à esquerda: para elementos pequenos
/// por natureza (um campo de data, um tile) que não devem esticar, mas precisam encolher em telas e
/// cards estreitos. Os blocos raiz dos previews não usam isto: ocupam a largura toda do card.
/// <para>
/// O MAUI não tem isso pronto: <c>MaximumWidthRequest</c> com <c>Fill</c> centraliza o elemento, com
/// <c>Start</c> ele encolhe até o conteúdo, e <c>WidthRequest</c> fixo estoura em espaços menores
/// (o mesmo preview aparece no card estreito da aba Lado a lado). Aqui a largura acompanha o pai.
/// </para>
/// <code>&lt;Border theming:Fit.MaxWidth="280"&gt;</code>
/// </summary>
public static class Fit
{
    public static readonly BindableProperty MaxWidthProperty = BindableProperty.CreateAttached(
        "MaxWidth", typeof(double), typeof(Fit), -1d, propertyChanged: OnMaxWidthChanged);

    public static double GetMaxWidth(BindableObject view) => (double)view.GetValue(MaxWidthProperty);

    public static void SetMaxWidth(BindableObject view, double value) => view.SetValue(MaxWidthProperty, value);

    private static void OnMaxWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view || newValue is not double max || max <= 0) return;

        // Alinhado à esquerda por padrão; quem pediu Center ou End (antes ou depois) é respeitado.
        if (view.HorizontalOptions.Alignment == LayoutAlignment.Fill) view.HorizontalOptions = LayoutOptions.Start;
        VisualElement? watched = null;

        void Update(object? sender, EventArgs e)
        {
            if (view.Parent is not VisualElement parent || parent.Width <= 0) return;
            var padding = parent is Layout layout ? layout.Padding.HorizontalThickness : 0;
            var available = parent.Width - padding - view.Margin.HorizontalThickness;
            var width = Math.Max(0, Math.Min(max, available));
            if (Math.Abs(view.WidthRequest - width) > 0.5) view.WidthRequest = width;
        }

        void Watch()
        {
            if (watched is not null) watched.SizeChanged -= Update;
            watched = view.Parent as VisualElement;
            if (watched is not null) watched.SizeChanged += Update;
            Update(null, EventArgs.Empty);
        }

        view.ParentChanged += (_, _) => Watch();
        Watch();
    }
}
