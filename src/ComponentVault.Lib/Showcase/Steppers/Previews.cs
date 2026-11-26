using System.Globalization;

namespace ComponentVault.Lib.Showcase.Steppers;

[ShowcasePreview("stepper.base")]
public partial class StepperBasePreview : ContentView
{
    public StepperBasePreview() => InitializeComponent();
}

[ShowcasePreview("stepper.styled")]
public partial class StepperStyledPreview : ContentView
{
    private const int Min = 1;
    private const int Max = 5;
    private const decimal UnitPrice = 89.90m;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private int _quantity = Min;

    public StepperStyledPreview()
    {
        InitializeComponent();
        Render();
    }

    private void OnMinus(object? sender, EventArgs e) => SetQuantity(_quantity - 1);

    private void OnPlus(object? sender, EventArgs e) => SetQuantity(_quantity + 1);

    private void SetQuantity(int value)
    {
        _quantity = Math.Clamp(value, Min, Max);
        Render();
    }

    private void Render()
    {
        QuantityLabel.Text = _quantity.ToString(PtBr);
        SemanticProperties.SetDescription(QuantityLabel, $"Quantidade {_quantity}");
        TotalLabel.Text = (UnitPrice * _quantity).ToString("C", PtBr);
        // Nos limites o botão é desabilitado, em vez de aceitar o toque e não fazer nada.
        MinusButton.IsEnabled = _quantity > Min;
        PlusButton.IsEnabled = _quantity < Max;
    }
}

[ShowcasePreview("stepper.syncfusion")]
public partial class StepperSyncfusionPreview : ContentView
{
    public StepperSyncfusionPreview() => InitializeComponent();
}
