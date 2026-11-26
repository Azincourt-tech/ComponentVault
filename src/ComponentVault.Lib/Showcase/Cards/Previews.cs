namespace ComponentVault.Lib.Showcase.Cards;

[ShowcasePreview("card.base")]
public partial class CardBasePreview : ContentView
{
    public CardBasePreview() => InitializeComponent();
}

[ShowcasePreview("card.styled")]
public partial class CardStyledPreview : ContentView
{
    public CardStyledPreview() => InitializeComponent();
}

[ShowcasePreview("card.expander")]
public partial class CardExpanderPreview : ContentView
{
    public CardExpanderPreview() => InitializeComponent();
}

[ShowcasePreview("card.syncfusion")]
public partial class CardSyncfusionPreview : ContentView
{
    public CardSyncfusionPreview() => InitializeComponent();

    // Os avisos dispensados voltam, para dar para testar de novo.
    private void OnRestoreClicked(object? sender, EventArgs e)
    {
        BuildCard.IsDismissed = false;
        ReviewCard.IsDismissed = false;
        QuotaCard.IsDismissed = false;
    }
}

[ShowcasePreview("card.accordion")]
public partial class CardAccordionPreview : ContentView
{
    public CardAccordionPreview() => InitializeComponent();
}
