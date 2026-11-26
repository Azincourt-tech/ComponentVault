using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Popups;

internal static class PageLocator
{
    /// <summary>Página que contém a View (sobe pela árvore); cai na página da janela ativa.</summary>
    public static Page? FindPage(Element element)
    {
        for (var e = element.Parent; e is not null; e = e.Parent)
        {
            if (e is Page page) return page;
        }

        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}

[ShowcasePreview("popup.base")]
public partial class PopupBasePreview : ContentView
{
    public PopupBasePreview() => InitializeComponent();

    // snippet:popup.base.code
    private async void OnAlertClicked(object? sender, EventArgs e)
    {
        if (PageLocator.FindPage(this) is not { } page) return;
        await page.DisplayAlertAsync("Atualização instalada", "A versão 2.4 já está ativa.", "OK");
        ResultLabel.Text = "Alerta fechado";
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (PageLocator.FindPage(this) is not { } page) return;
        var confirmed = await page.DisplayAlertAsync("Excluir projeto?", "Esta ação não pode ser desfeita.", "Excluir", "Cancelar");
        ResultLabel.Text = confirmed ? "Confirmado: excluir" : "Cancelado";
    }

    private async void OnActionSheetClicked(object? sender, EventArgs e)
    {
        if (PageLocator.FindPage(this) is not { } page) return;
        var choice = await page.DisplayActionSheetAsync("Projeto Atlas", "Cancelar", "Excluir", "Compartilhar", "Duplicar", "Renomear");
        ResultLabel.Text = $"Escolha: {choice ?? "nenhuma"}";
    }
    // /snippet:popup.base.code
}

[ShowcasePreview("popup.toolkit")]
public partial class PopupToolkitPreview : ContentView
{
    public PopupToolkitPreview() => InitializeComponent();

    // snippet:popup.toolkit.code
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (PageLocator.FindPage(this) is not { } page) return;

        var dialog = new ConfirmDialog(
            title: "Excluir “Projeto Atlas”?",
            message: "As 18 tarefas e os arquivos anexados serão apagados para os 4 membros. Isso não pode ser desfeito.",
            confirmText: "Excluir projeto");

        var shape = new RoundRectangle { CornerRadius = 16, StrokeThickness = 1 };
        shape.SetThemeBrush(Shape.StrokeProperty, "Hairline");

        var result = await page.ShowPopupAsync<bool>(dialog, new PopupOptions
        {
            Shape = shape,
            Shadow = null,
            PageOverlayColor = Colors.Black.WithAlpha(0.45f),
            CanBeDismissedByTappingOutsideOfPopup = true,
        });

        ResultLabel.Text = result.WasDismissedByTappingOutsideOfPopup
            ? "Fechado tocando fora: nada foi excluído"
            : result.Result ? "Confirmado: projeto excluído" : "Cancelado: nada foi excluído";
    }
    // /snippet:popup.toolkit.code
}

[ShowcasePreview("popup.syncfusion")]
public partial class PopupSyncfusionPreview : ContentView
{
    private bool _accepted;

    public PopupSyncfusionPreview()
    {
        InitializeComponent();
        // O botão de excluir confirma; qualquer outra saída (cancelar, tocar fora, Esc) é cancelar.
        DeletePopup.AcceptCommand = new Command(() => { _accepted = true; DeletePopup.IsOpen = false; });
    }

    private void OnOpenClicked(object? sender, EventArgs e)
    {
        _accepted = false;
        DeletePopup.IsOpen = true;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        ResultLabel.Text = _accepted ? "Componente excluído. Dá para desfazer em até 30 dias." : "Exclusão cancelada.";
        ResultLabel.SetThemeColor(Label.TextColorProperty, _accepted ? "DangerText" : "InkSecondary");
    }
}

[ShowcasePreview("popup.custom")]
public partial class PopupCustomPreview : ContentView
{
    public PopupCustomPreview() => InitializeComponent();

    private void OnShareClicked(object? sender, EventArgs e) => ShareSheet.Open();

    private void OnCancelClicked(object? sender, EventArgs e) => ShareSheet.Close();

    // Escolher uma opção fecha a folha e mostra o resultado na tela de baixo.
    private void OnShareOption(object? sender, TappedEventArgs e)
    {
        ResultLabel.Text = e.Parameter as string;
        ShareSheet.Close();
    }
}
