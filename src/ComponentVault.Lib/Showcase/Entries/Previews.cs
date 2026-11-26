using System.Net.Mail;
using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Entries;

[ShowcasePreview("entry.base")]
public partial class EntryBasePreview : ContentView
{
    public EntryBasePreview() => InitializeComponent();
}

[ShowcasePreview("entry.styled")]
public partial class EntryStyledPreview : ContentView
{
    private const string PasswordHint = "Mínimo de 8 caracteres";
    private bool _submitted;

    public EntryStyledPreview() => InitializeComponent();

    // Antes do primeiro envio não há erro na tela; depois dele, cada campo revalida ao digitar
    // para o erro sumir assim que for corrigido. (Num app real isto mora na ViewModel.)
    private void OnFieldChanged(object? sender, TextChangedEventArgs e)
    {
        if (_submitted) Validate();
    }

    private void OnPasswordChanged(object? sender, TextChangedEventArgs e)
    {
        OnFieldChanged(sender, e);
        UpdateStrength(e.NewTextValue ?? string.Empty);
    }

    // Força simples: comprimento e variedade de caracteres. Num app real, use uma lib como zxcvbn.
    private void UpdateStrength(string password)
    {
        var score = password.Length == 0 ? 0 : 1;
        if (password.Length >= 8) score++;
        if (password.Any(char.IsDigit) && password.Any(char.IsLetter)) score++;
        if (password.Length >= 12 && password.Any(c => !char.IsLetterOrDigit(c))) score++;

        var (key, text) = score switch
        {
            0 => ("HairlineStrong", "Força da senha"),
            1 => ("Danger", "Fraca"),
            2 => ("Warning", "Razoável"),
            3 => ("Accent", "Boa"),
            _ => ("Success", "Forte"),
        };

        BoxView[] segments = [Strength1, Strength2, Strength3, Strength4];
        for (var i = 0; i < segments.Length; i++)
            segments[i].SetThemeColor(BoxView.ColorProperty, i < score ? key : "HairlineStrong");
        StrengthLabel.Text = text;
        StrengthLabel.SetThemeColor(Label.TextColorProperty, score == 0 ? "InkTertiary" : score == 1 ? "DangerText" : score == 4 ? "SuccessText" : "InkSecondary");
    }

    private void OnSubmit(object? sender, EventArgs e)
    {
        _submitted = true;
        var errors = Validate();

        StatusLabel.IsVisible = true;
        StatusLabel.Text = errors == 0
            ? "Conta criada. Enviamos um link de confirmação para o seu e-mail."
            : errors == 1 ? "Corrija 1 campo para continuar." : $"Corrija {errors} campos para continuar.";
        StatusLabel.SetThemeColor(Label.TextColorProperty, errors == 0 ? "SuccessText" : "DangerText");
    }

    private int Validate()
    {
        var errors = 0;
        errors += Check(NameField, !string.IsNullOrWhiteSpace(NameField.Text), "Informe seu nome.");
        errors += Check(EmailField, IsEmail(EmailField.Text), "Use um e-mail válido, como voce@exemplo.com.");
        errors += Check(PasswordField, (PasswordField.Text?.Length ?? 0) >= 8, "A senha precisa de pelo menos 8 caracteres.", PasswordHint);
        return errors;
    }

    private static int Check(VaultTextField field, bool valid, string error, string? hint = null)
    {
        field.State = valid ? FieldState.Normal : FieldState.Error;
        field.Hint = valid ? hint : error;
        return valid ? 0 : 1;
    }

    private static bool IsEmail(string? text) =>
        !string.IsNullOrWhiteSpace(text) && MailAddress.TryCreate(text, out var address) && address.Host.Contains('.');
}

[ShowcasePreview("entry.uranium")]
public partial class EntryUraniumPreview : ContentView
{
    public EntryUraniumPreview() => InitializeComponent();
}
