using CommunityToolkit.Maui.Views;
using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Popups;

// snippet:control.confirmdialog
/// <summary>
/// Diálogo de confirmação sobre o Popup&lt;bool&gt; do CommunityToolkit.Maui.
/// Título diz a ação, o texto diz a consequência e o botão destrutivo repete o verbo.
/// </summary>
public sealed class ConfirmDialog : Popup<bool>
{
    public ConfirmDialog(string title, string message, string confirmText, string cancelText = "Cancelar", bool isDestructive = true)
    {
        WidthRequest = 380;
        Padding = new Thickness(24);
        this.SetThemeColor(BackgroundColorProperty, "Surface");

        var icon = new Border
        {
            WidthRequest = 40,
            HeightRequest = 40,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = "!",
                FontFamily = "JakartaBold",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
        };
        icon.SetThemeColor(BackgroundColorProperty, isDestructive ? "DangerTint" : "InfoTint");
        ((Label)icon.Content).SetThemeColor(Label.TextColorProperty, isDestructive ? "DangerText" : "AccentText");

        var titleLabel = new Label { Text = title, FontFamily = "JakartaSemiBold", FontSize = 19 };
        titleLabel.SetThemeColor(Label.TextColorProperty, "Ink");

        var messageLabel = new Label { Text = message, FontFamily = "JakartaRegular", FontSize = 15, LineHeight = 1.4 };
        messageLabel.SetThemeColor(Label.TextColorProperty, "InkSecondary");

        var cancel = new Button { Text = cancelText, Style = Resource<Style>("SecondaryButton") };
        cancel.Clicked += async (_, _) => await CloseAsync(false);

        var confirm = new Button { Text = confirmText, Style = Resource<Style>(isDestructive ? "DangerButton" : "PrimaryButton") };
        confirm.Clicked += async (_, _) => await CloseAsync(true);

        // Ações com a mesma largura, a confirmação à direita (ordem esperada no Windows e no Android).
        var actions = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 10, Margin = new Thickness(0, 8, 0, 0) };
        actions.Add(cancel, 0);
        actions.Add(confirm, 1);

        Content = new VerticalStackLayout { Spacing = 12, Children = { icon, titleLabel, messageLabel, actions } };
        SemanticProperties.SetDescription(this, title);
    }

    private static T Resource<T>(string key) => (T)Application.Current!.Resources[key];
}
// /snippet:control.confirmdialog
