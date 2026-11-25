using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Toasts;

[ShowcasePreview("toast.custom")]
public partial class ToastCustomPreview : ContentView
{
    private readonly List<string> _items = ["Proposta do redesenho", "Notas da reunião de produto", "Checklist de publicação"];

    public ToastCustomPreview()
    {
        InitializeComponent();
        RenderItems();
    }

    private async void OnSaveClicked(object? sender, EventArgs e) =>
        await Toast.ShowAsync("Alterações salvas", ToastKind.Success);

    private async void Delete(string item)
    {
        var index = _items.IndexOf(item);
        _items.RemoveAt(index);
        RenderItems();

        await Toast.ShowAsync($"“{item}” excluído", ToastKind.Error, "Desfazer", () =>
        {
            _items.Insert(Math.Min(index, _items.Count), item);
            RenderItems();
        });
    }

    private void RenderItems()
    {
        ItemList.Children.Clear();
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (i > 0)
            {
                var divider = new BoxView { HeightRequest = 1, Margin = new Thickness(16, 0) };
                divider.SetThemeColor(BoxView.ColorProperty, "Hairline");
                ItemList.Children.Add(divider);
            }

            var row = new Grid
            {
                ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
                Padding = new Thickness(16, 4, 4, 4),
                MinimumHeightRequest = 52,
            };
            var label = new Label { Text = item, FontFamily = "JakartaRegular", FontSize = 16, VerticalOptions = LayoutOptions.Center };
            label.SetThemeColor(Label.TextColorProperty, "Ink");
            var delete = new Button
            {
                Text = "Excluir",
                Style = (Style)Application.Current!.Resources["TertiaryButton"],
            };
            delete.SetThemeColor(Button.TextColorProperty, "DangerText");
            SemanticProperties.SetDescription(delete, $"Excluir {item}");
            delete.Clicked += (_, _) => Delete(item);
            row.Add(label, 0);
            row.Add(delete, 1);
            ItemList.Children.Add(row);
        }

        if (_items.Count == 0)
        {
            var empty = new Label { Text = "Nenhum rascunho.", FontFamily = "JakartaRegular", FontSize = 14, Padding = new Thickness(16, 16) };
            empty.SetThemeColor(Label.TextColorProperty, "InkTertiary");
            ItemList.Children.Add(empty);
        }
    }
}

[ShowcasePreview("toast.toolkit")]
public partial class ToastToolkitPreview : ContentView
{
    public ToastToolkitPreview() => InitializeComponent();

    private Task ShowAsync(string text, ToastDuration duration) => ShowToastAsync(text, duration);

    private async void OnShortClicked(object? sender, EventArgs e) =>
        await ShowAsync("Componente copiado", ToastDuration.Short);

    private async void OnLongClicked(object? sender, EventArgs e) =>
        await ShowAsync("Sincronização concluída: 12 componentes atualizados", ToastDuration.Long);

    // snippet:toast.toolkit
    private async Task ShowToastAsync(string text, ToastDuration duration)
    {
        try
        {
            var toast = Toast.Make(text, duration, textSize: 15);
            await toast.Show();
            ResultLabel.IsVisible = false;
        }
        catch (Exception ex)
        {
            // No Windows sem empacotamento, as notificações podem estar indisponíveis.
            ResultLabel.Text = $"Toast indisponível nesta plataforma: {ex.Message}";
            ResultLabel.IsVisible = true;
        }
    }
    // /snippet:toast.toolkit
}
