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

[ShowcasePreview("button.syncfusion")]
public partial class ButtonSyncfusionPreview : ContentView
{
    public ButtonSyncfusionPreview()
    {
        InitializeComponent();
        // DashArray é float[] e o XAML não converte a partir de texto.
        DropZone.DashArray = [5f, 4f];
    }

    // O texto diz o estado atual; ligado, o botão fica preenchido no tom do destaque.
    private void OnFollowClicked(object? sender, EventArgs e)
    {
        var following = FollowButton.IsChecked;
        FollowButton.Text = following ? "Seguindo" : "Seguir";
        FollowButton.Background = following
            ? new SolidColorBrush(ComponentVault.Lib.Theming.ThemeColors.Resolve("AccentTint", Colors.Transparent))
            : Brush.Transparent;
        FollowButton.TextColor = ComponentVault.Lib.Theming.ThemeColors.Resolve(following ? "AccentText" : "Ink", Colors.Black);
    }
}
