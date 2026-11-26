namespace ComponentVault.Lib.Showcase.Buttons;

[ShowcasePreview("button.base")]
public partial class ButtonBasePreview : ContentView
{
    private int _clicks;

    public ButtonBasePreview() => InitializeComponent();

    private void OnClicked(object? sender, EventArgs e) =>
        CounterLabel.Text = ++_clicks == 1 ? "1 clique" : $"{_clicks} cliques";
}

[ShowcasePreview("button.styled")]
public partial class ButtonStyledPreview : ContentView
{
    private const string IdleText = "Salvar alterações";
    private bool _busy;

    public ButtonStyledPreview() => InitializeComponent();

    // Simula uma gravação de 1,5s. Num app real o Command (AsyncRelayCommand) já bloqueia
    // reentrada; aqui o flag faz esse papel.
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_busy) return;

        _busy = true;
        SaveButton.InputTransparent = true;
        SaveButton.Text = "Salvando…";
        // Abre espaço à esquerda para o indicador só enquanto carrega; parado, o texto fica centrado.
        SaveButton.Padding = new Thickness(44, 11, 20, 11);
        SaveSpinner.IsVisible = SaveSpinner.IsRunning = true;

        await Task.Delay(1500);

        SaveSpinner.IsVisible = SaveSpinner.IsRunning = false;
        SaveButton.Padding = new Thickness(32, 11);
        SaveButton.Text = "Salvo";
        await Task.Delay(1200);

        SaveButton.Text = IdleText;
        SaveButton.InputTransparent = false;
        _busy = false;
    }
}

[ShowcasePreview("button.uranium")]
public partial class ButtonUraniumPreview : ContentView
{
    public ButtonUraniumPreview() => InitializeComponent();
}
