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

[ShowcasePreview("otp.syncfusion")]
public partial class OtpSyncfusionPreview : ContentView
{
    public OtpSyncfusionPreview() => InitializeComponent();

    // Com as 6 caixas preenchidas, a dica vira confirmação (num app real, aqui o código é enviado).
    private void OnValueChanged(object? sender, Syncfusion.Maui.Toolkit.OtpInput.OtpInputValueChangedEventArgs e)
    {
        var complete = e.NewValue?.Length == SmsCode.Length;
        SmsHint.Text = complete ? "Código completo. Verificando..." : "Cole o código inteiro ou digite um dígito por caixa.";
        SmsHint.SetThemeColor(Label.TextColorProperty, complete ? "SuccessText" : "InkTertiary");
    }
}
