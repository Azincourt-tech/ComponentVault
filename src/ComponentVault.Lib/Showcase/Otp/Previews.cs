using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Otp;

[ShowcasePreview("otp.custom")]
public partial class OtpCustomPreview : ContentView
{
    private const string DemoCode = "123456";

    public OtpCustomPreview() => InitializeComponent();

    private void OnCompleted(object? sender, string code)
    {
        var ok = code == DemoCode;
        CodeInput.HasError = !ok;
        StatusLabel.IsVisible = true;
        StatusLabel.Text = ok ? "Código confirmado." : "Código incorreto. Confira os dígitos e tente de novo.";
        StatusLabel.SetThemeColor(Label.TextColorProperty, ok ? "SuccessText" : "DangerText");
    }

    // Reenviar limpa o campo, volta o foco e trava o botão por 30s (evita spam de SMS/e-mail).
    private async void OnResend(object? sender, EventArgs e)
    {
        CodeInput.Code = string.Empty;
        CodeInput.Focus();
        StatusLabel.IsVisible = false;

        ResendButton.IsEnabled = false;
        for (var s = 30; s > 0; s--)
        {
            ResendButton.Text = $"Reenviar em {s}s";
            await Task.Delay(1000);
        }

        ResendButton.Text = "Reenviar código";
        ResendButton.IsEnabled = true;
    }
}
