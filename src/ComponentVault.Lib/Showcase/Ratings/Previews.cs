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
