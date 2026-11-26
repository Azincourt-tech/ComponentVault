using System.ComponentModel;

namespace ComponentVault.Lib.Showcase.Ratings;

[ShowcasePreview("rating.custom")]
public partial class RatingCustomPreview : ContentView
{
    private static readonly string[] Meanings = ["Sem nota", "Ruim", "Regular", "Bom", "Muito bom", "Excelente"];

    public RatingCustomPreview() => InitializeComponent();

    private void OnRatingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RatingView.Value) || sender is not RatingView rating || MeaningLabel is null) return;
        MeaningLabel.Text = Meanings[Math.Clamp((int)Math.Ceiling(rating.Value), 0, Meanings.Length - 1)];
    }
}

[ShowcasePreview("rating.base")]
public partial class RatingBasePreview : ContentView
{
    public RatingBasePreview() => InitializeComponent();

    // Cada botão marca a própria posição: os anteriores ficam com estrela cheia, os seguintes vazia.
    private void OnStarClicked(object? sender, EventArgs e)
    {
        var buttons = Stars.Children.OfType<Button>().ToList();
        var value = buttons.IndexOf((Button)sender!) + 1;
        for (var i = 0; i < buttons.Count; i++) buttons[i].Text = i < value ? "★" : "☆";
        ValueLabel.Text = $"Nota: {value} de 5";
    }
}

[ShowcasePreview("rating.toolkit")]
public partial class RatingToolkitPreview : ContentView
{
    private static readonly string[] Meanings = ["Sem nota", "Ruim", "Regular", "Bom", "Muito bom", "Excelente"];

    public RatingToolkitPreview() => InitializeComponent();

    private void OnRatingChanged(object? sender, CommunityToolkit.Maui.Core.RatingChangedEventArgs e)
    {
        if (MeaningLabel is null) return;
        MeaningLabel.Text = Meanings[Math.Clamp((int)Math.Ceiling(e.Rating), 0, Meanings.Length - 1)];
    }
}
