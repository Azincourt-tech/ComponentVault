using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Pickers;

// snippet:control.wheelpicker
/// <summary>
/// Roda de opções, como no iOS: as opções formam um cilindro, com a do meio maior e nítida e as
/// vizinhas menores, apagadas e inclinadas (RotationX). Arrastar gira a roda, que encaixa na opção
/// mais próxima ao soltar; tocar numa vizinha a traz para o centro. Mostra o contexto que a lista
/// do Picker esconde: o que vem antes e depois.
/// </summary>
public class WheelPicker : ContentView
{
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(WheelPicker), 0, BindingMode.TwoWay,
        propertyChanged: (b, _, n) => ((WheelPicker)b).OnSelectedIndexChanged((int)n));

    private const double ItemHeight = 40;
    private const int VisibleRows = 5;
    private const string SnapAnimation = "WheelSnap";

    private readonly Grid _wheel;
    private readonly List<Label> _labels = [];
    private IReadOnlyList<string> _items = [];
    private double _position;      // índice "contínuo" no centro (fracionário durante o arrasto)
    private double _panStart;

    public event EventHandler? SelectionChanged;

    public WheelPicker()
    {
        var band = new Border
        {
            HeightRequest = ItemHeight,
            VerticalOptions = LayoutOptions.Center,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            InputTransparent = true,
        };
        band.SetThemeColor(BackgroundColorProperty, "AccentTint");

        _wheel = new Grid { HeightRequest = ItemHeight * VisibleRows, IsClippedToBounds = true, Children = { band } };
        Content = _wheel;

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        _wheel.GestureRecognizers.Add(pan);
    }

    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

    public string? SelectedItem => SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex] : null;

    public IReadOnlyList<string> Items
    {
        get => _items;
        set
        {
            _items = value;
            foreach (var old in _labels) _wheel.Children.Remove(old);
            _labels.Clear();
            for (var i = 0; i < _items.Count; i++)
            {
                var index = i;
                var label = new Label
                {
                    Text = _items[i],
                    HeightRequest = ItemHeight,
                    FontSize = 18,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                };
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => SnapTo(index);
                label.GestureRecognizers.Add(tap);
                _labels.Add(label);
                _wheel.Children.Add(label);
            }
            _position = Math.Clamp(SelectedIndex, 0, Math.Max(0, _items.Count - 1));
            Layout();
        }
    }

    // Cada opção fica a (i - posição) linhas do centro; quanto mais longe, menor, mais apagada e inclinada.
    private void Layout()
    {
        for (var i = 0; i < _labels.Count; i++)
        {
            var label = _labels[i];
            var distance = i - _position;
            var far = Math.Min(Math.Abs(distance), 3);
            label.TranslationY = distance * ItemHeight * (1 - far * 0.04);
            label.Scale = 1 - far * 0.09;
            label.Opacity = Math.Max(0, 1 - far * 0.32);
            label.RotationX = Math.Clamp(distance * 18, -70, 70);
            label.IsVisible = Math.Abs(distance) < 3.2;
            var selected = Math.Abs(distance) < 0.5;
            label.FontFamily = selected ? "JakartaSemiBold" : "JakartaRegular";
            label.SetThemeColor(Label.TextColorProperty, selected ? "Ink" : "InkSecondary");
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                this.AbortAnimation(SnapAnimation);
                _panStart = _position;
                break;
            case GestureStatus.Running:
                // Arrastar para cima avança; um pouco de folga além das pontas, que volta ao soltar.
                _position = Math.Clamp(_panStart - e.TotalY / ItemHeight, -0.4, _items.Count - 0.6);
                Layout();
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                SnapTo((int)Math.Round(_position));
                break;
        }
    }

    private void SnapTo(int index)
    {
        if (_items.Count == 0) return;
        index = Math.Clamp(index, 0, _items.Count - 1);
        var from = _position;
        this.AbortAnimation(SnapAnimation);
        this.Animate(SnapAnimation, t => { _position = from + (index - from) * t; Layout(); },
            length: (uint)Math.Clamp(Math.Abs(index - from) * 90, 120, 360), easing: Easing.CubicOut,
            finished: (_, cancelled) => { if (!cancelled) SelectedIndex = index; });
    }

    private void OnSelectedIndexChanged(int index)
    {
        if (Math.Abs(_position - index) > 0.01 && !this.AnimationIsRunning(SnapAnimation))
        {
            _position = index;
            Layout();
        }
        SemanticProperties.SetDescription(this, SelectedItem);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
// /snippet:control.wheelpicker
