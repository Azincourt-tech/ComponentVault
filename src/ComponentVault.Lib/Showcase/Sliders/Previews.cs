using MauiIcons.Fluent;
using System.Globalization;

namespace ComponentVault.Lib.Showcase.Sliders;

[ShowcasePreview("slider.base")]
public partial class SliderBasePreview : ContentView
{
    public SliderBasePreview() => InitializeComponent();
}

[ShowcasePreview("slider.styled")]
public partial class SliderStyledPreview : ContentView
{
    private double _beforeMute = 65;

    public SliderStyledPreview() => InitializeComponent();

    // Encaixe em passos: o valor exibido e o salvo são sempre "redondos".
    private void OnVolumeChanged(object? sender, ValueChangedEventArgs e)
    {
        Snap(sender, e.NewValue, 5);
        var value = VolumeSlider.Value;
        VolumeValue.Text = value <= 0 ? "Mudo" : $"{value:F0}%";
        // O ícone da esquerda acompanha o nível: cortado no mudo, uma onda no volume baixo.
        MuteIcon.Icon = value <= 0 ? FluentIcons.SpeakerMute20 : value < 50 ? FluentIcons.Speaker120 : FluentIcons.Speaker020;
    }

    private void OnMuteTapped(object? sender, TappedEventArgs e)
    {
        if (VolumeSlider.Value > 0)
        {
            _beforeMute = VolumeSlider.Value;
            VolumeSlider.Value = 0;
        }
        else
        {
            VolumeSlider.Value = _beforeMute > 0 ? _beforeMute : 50;
        }
    }

    private void OnFontChanged(object? sender, ValueChangedEventArgs e) => Snap(sender, e.NewValue, 1);

    private static void Snap(object? sender, double value, double step)
    {
        if (sender is not Slider slider) return;
        var snapped = Math.Round(value / step) * step;
        if (Math.Abs(snapped - value) > double.Epsilon) slider.Value = snapped;
    }
}

[ShowcasePreview("slider.custom")]
public partial class SliderCustomPreview : ContentView
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public SliderCustomPreview()
    {
        InitializeComponent();
        UpdateLabel();
    }

    private void OnRangeChanged(object? sender, EventArgs e) => UpdateLabel();

    private void UpdateLabel()
    {
        if (RangeLabel is null || PriceRange is null) return;
        RangeLabel.Text = string.Create(PtBr, $"R$ {PriceRange.LowerValue:N0} – R$ {PriceRange.UpperValue:N0}");
    }
}
