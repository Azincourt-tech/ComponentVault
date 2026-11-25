using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Segmented;

// snippet:control.segmented
/// <summary>
/// Controle segmentado: trilha neutra com um indicador que desliza até a opção escolhida
/// (200ms, sem bounce). Segmentos de largura igual e alvo de toque de 36px ou mais.
/// </summary>
public class SegmentedControl : ContentView
{
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items), typeof(IList<string>), typeof(SegmentedControl), null,
        propertyChanged: (b, _, _) => ((SegmentedControl)b).Rebuild());

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(SegmentedControl), 0, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((SegmentedControl)b).OnSelectedIndexChanged());

    private readonly Grid _grid = new() { ColumnSpacing = 0 };
    private readonly List<Label> _labels = [];
    private readonly Border _indicator = new()
    {
        StrokeShape = new RoundRectangle { CornerRadius = 8 },
        StrokeThickness = 1,
    };

    public SegmentedControl()
    {
        _indicator.SetThemeColor(BackgroundColorProperty, "Surface");
        _indicator.SetThemeBrush(Border.StrokeProperty, "Hairline");

        var track = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            StrokeThickness = 0,
            Padding = 3,
            Content = _grid,
        };
        track.SetThemeColor(BackgroundColorProperty, "SurfaceAlt");

        _grid.SizeChanged += (_, _) => MoveIndicator(animated: false);
        Content = track;
    }

    public event EventHandler<int>? SelectionChanged;

    public IList<string>? Items
    {
        get => (IList<string>?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private void Rebuild()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _labels.Clear();

        var items = Items ?? [];
        foreach (var _ in items) _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // O indicador vem primeiro para ficar atrás dos textos.
        _grid.Add(_indicator, 0, 0);

        for (var i = 0; i < items.Count; i++)
        {
            var index = i;
            var label = new Label
            {
                Text = items[i],
                FontFamily = "JakartaMedium",
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(12, 8),
                MinimumHeightRequest = 36,
            };
            label.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => SelectedIndex = index) });
            _labels.Add(label);
            _grid.Add(label, i, 0);
        }

        UpdateLabels();
        MoveIndicator(animated: false);
    }

    private void OnSelectedIndexChanged()
    {
        UpdateLabels();
        MoveIndicator(animated: true);
        SelectionChanged?.Invoke(this, SelectedIndex);
    }

    private void UpdateLabels()
    {
        for (var i = 0; i < _labels.Count; i++)
        {
            var selected = i == SelectedIndex;
            _labels[i].SetThemeColor(Label.TextColorProperty, selected ? "Ink" : "InkSecondary");
            _labels[i].FontFamily = selected ? "JakartaSemiBold" : "JakartaMedium";
            SemanticProperties.SetDescription(_labels[i],
                $"{_labels[i].Text}, {i + 1} de {_labels.Count}{(selected ? ", selecionado" : string.Empty)}");
        }
    }

    private void MoveIndicator(bool animated)
    {
        if (_labels.Count == 0 || _grid.Width <= 0) return;

        var x = Math.Clamp(SelectedIndex, 0, _labels.Count - 1) * (_grid.Width / _labels.Count);
        _indicator.CancelAnimations();
        if (animated && Handler is not null)
        {
            _ = _indicator.TranslateToAsync(x, 0, 200, Easing.CubicOut);
        }
        else
        {
            _indicator.TranslationX = x;
        }
    }
}
// /snippet:control.segmented
