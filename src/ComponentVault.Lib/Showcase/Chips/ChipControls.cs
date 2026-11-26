using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Chips;

// snippet:control.choicechips
/// <summary>
/// Chips de escolha única com uma pílula só, que desliza até o chip tocado (mola na posição,
/// largura sem quicar). O texto do escolhido ganha a cor de destaque; os outros ficam neutros.
/// </summary>
public class ChoiceChipGroup : ContentView
{
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(ChoiceChipGroup), 0, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((ChoiceChipGroup)b).OnSelectionChanged());

    private const double ChipHeight = 36;

    private readonly HorizontalStackLayout _row = new() { Spacing = 4 };
    private readonly SlidingIndicator _slider;
    private readonly List<Label> _labels = [];
    private IList<string> _options = [];

    public ChoiceChipGroup()
    {
        var pill = new Border
        {
            HeightRequest = ChipHeight,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ChipHeight / 2 },
        };
        pill.SetThemeColor(BackgroundColorProperty, "AccentTint");
        pill.SetThemeColor(Border.StrokeProperty, "Accent");

        // Trilho neutro em volta: a pílula corre dentro dele.
        var track = new Border
        {
            Padding = 4,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = (ChipHeight / 2) + 4 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Grid { Children = { pill, _row } },
        };
        track.SetThemeColor(BackgroundColorProperty, "SurfaceAlt");
        Content = track;

        _slider = new SlidingIndicator(pill, _row);
        _slider.Measured += (_, _) => { if (_slider.Target is null) _slider.JumpTo(SelectedView()); };
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Textos dos chips, na ordem.</summary>
    public IList<string> Options
    {
        get => _options;
        set
        {
            _options = value;
            _row.Clear();
            _labels.Clear();
            for (var i = 0; i < value.Count; i++)
            {
                var index = i;
                var label = new Label
                {
                    Text = value[i],
                    FontFamily = "JakartaMedium",
                    FontSize = 14,
                    Padding = new Thickness(16, 0),
                    HeightRequest = ChipHeight,
                    VerticalTextAlignment = TextAlignment.Center,
                };
                SemanticProperties.SetDescription(label, value[i]);
                Motion.SetPress(label, true);
                label.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => SelectedIndex = index) });
                _labels.Add(label);
                _row.Add(label);
            }
            Paint();
        }
    }

    private View? SelectedView() => SelectedIndex >= 0 && SelectedIndex < _labels.Count ? _labels[SelectedIndex] : null;

    private void OnSelectionChanged()
    {
        Paint();
        _slider.SlideTo(SelectedView());
    }

    private void Paint()
    {
        for (var i = 0; i < _labels.Count; i++)
        {
            _labels[i].SetThemeColor(Label.TextColorProperty, i == SelectedIndex ? "AccentText" : "InkSecondary");
        }
    }
}
// /snippet:control.choicechips

// snippet:control.filterchip
/// <summary>
/// Chip de filtro que se transforma ao ligar: abre espaço para o check, que aparece crescendo,
/// e o chip dá um pulo curto. Desligar desfaz o caminho. O estado não depende só da cor.
/// </summary>
public class FilterChip : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(FilterChip), null,
        propertyChanged: (b, _, n) => ((FilterChip)b)._label.Text = (string?)n);

    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(FilterChip), false, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((FilterChip)b).Update(animated: true));

    private const double CheckSize = 16;
    private const double CheckGap = 6;
    private const uint Length = 260;

    private readonly Border _frame = new()
    {
        HeightRequest = 36,
        Padding = new Thickness(14, 0),
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 18 },
    };
    private readonly MauiIcon _check = new()
    {
        Icon = FluentIcons.Checkmark16,
        IconSize = CheckSize,
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.Start,
    };
    private readonly Grid _checkSlot = new() { WidthRequest = 0, IsClippedToBounds = true, VerticalOptions = LayoutOptions.Center };
    private readonly Label _label = new() { FontFamily = "JakartaMedium", FontSize = 14, VerticalOptions = LayoutOptions.Center };

    public FilterChip()
    {
        _check.SetThemeColor(MauiIcon.IconColorProperty, "AccentText");
        _checkSlot.Add(_check);
        _frame.Content = new HorizontalStackLayout { Children = { _checkSlot, _label } };
        _frame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => IsChecked = !IsChecked) });
        Motion.SetPress(_frame, true);
        Content = _frame;
        Update(animated: false);
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private void Update(bool animated)
    {
        var on = IsChecked;
        _frame.SetThemeColor(BackgroundColorProperty, on ? "AccentTint" : "Surface");
        _frame.SetThemeColor(Border.StrokeProperty, on ? "AccentText" : "HairlineStrong");
        _label.SetThemeColor(Label.TextColorProperty, on ? "AccentText" : "Ink");
        SemanticProperties.SetDescription(this, $"{Text}, {(on ? "ligado" : "desligado")}");

        var width = on ? CheckSize + CheckGap : 0;
        if (!animated)
        {
            _checkSlot.WidthRequest = width;
            _check.Opacity = on ? 1 : 0;
            _check.Scale = on ? 1 : 0.4;
            return;
        }

        // O espaço abre primeiro e o check cresce dentro dele; o chip todo dá um pulo de 6%.
        _checkSlot.AbortAnimation("FilterChipSlot");
        new Animation(v => _checkSlot.WidthRequest = v, _checkSlot.WidthRequest, width, Easing.CubicOut)
            .Commit(_checkSlot, "FilterChipSlot", rate: 16, length: Length);
        _ = _check.FadeToAsync(on ? 1 : 0, Length, Easing.CubicOut);
        _ = _check.ScaleToAsync(on ? 1 : 0.4, Length, Easing.SpringOut);
        if (on) _ = PopAsync();
    }

    private async Task PopAsync()
    {
        await _frame.ScaleToAsync(1.06, 110, Easing.CubicOut);
        await _frame.ScaleToAsync(1, 200, Easing.SpringOut);
    }
}
// /snippet:control.filterchip
