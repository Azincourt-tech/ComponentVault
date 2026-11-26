namespace ComponentVault.Lib.Showcase.Segmented;

[ShowcasePreview("segmented.custom")]
public partial class SegmentedCustomPreview : ContentView
{
    private static readonly (string Value, string Delta)[] Metrics =
    [
        ("214", "+4% vs. ontem"),
        ("1.284", "+12% vs. semana anterior"),
        ("5.902", "−3% vs. mês anterior"),
        ("61.340", "+38% vs. ano anterior"),
    ];

    public SegmentedCustomPreview() => InitializeComponent();

    private void OnPeriodChanged(object? sender, int index)
    {
        if (MetricLabel is null || DeltaLabel is null || index < 0 || index >= Metrics.Length) return;
        (MetricLabel.Text, DeltaLabel.Text) = Metrics[index];
    }
}

[ShowcasePreview("segmented.sharpnado")]
public partial class SegmentedSharpnadoPreview : ContentView
{
    public SegmentedSharpnadoPreview() => InitializeComponent();
}

[ShowcasePreview("segmented.syncfusion")]
public partial class SegmentedSyncfusionPreview : ContentView
{
    private static readonly string[] Periods = ["o dia de hoje", "a semana atual", "o mês atual", "o ano atual"];

    public SegmentedSyncfusionPreview() => InitializeComponent();

    private void OnSelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        var index = PeriodSegments.SelectedIndex ?? -1;
        if (index >= 0 && index < Periods.Length) PeriodLabel.Text = $"Mostrando {Periods[index]}.";
    }
}
