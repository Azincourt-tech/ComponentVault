using System.Collections.ObjectModel;

namespace ComponentVault.Lib.Showcase.Lists;

[ShowcasePreview("list.base")]
public partial class ListBasePreview : ContentView
{
    public ListBasePreview() => InitializeComponent();
}

public sealed record InboxItem(string Sender, string Subject, string Time)
{
    public string Initials => string.Concat(Sender.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0]));
}

[ShowcasePreview("list.styled")]
public partial class ListStyledPreview : ContentView
{
    // snippet:list.styled.code
    private (InboxItem Item, int Index)? _lastDeleted;

    public ObservableCollection<InboxItem> Messages { get; } =
    [
        new("Ana Souza", "Revisão do contrato com os ajustes da cláusula 4", "09:42"),
        new("Bruno Lima", "Fotos do evento de sábado", "Ontem"),
        new("Carla Mendes", "Orçamento aprovado, podemos seguir", "Seg"),
    ];

    private void OnDeleteInvoked(object? sender, EventArgs e) =>
        Delete((sender as BindableObject)?.BindingContext as InboxItem);

    private void OnDeleteMenuClicked(object? sender, EventArgs e) =>
        Delete((sender as BindableObject)?.BindingContext as InboxItem);

    // Excluir sem perguntar, mas sempre com "Desfazer": menos atrito e nenhum dado perdido por engano.
    private void Delete(InboxItem? item)
    {
        if (item is null) return;

        var index = Messages.IndexOf(item);
        if (index < 0) return;

        Messages.RemoveAt(index);
        _lastDeleted = (item, index);
        UndoLabel.Text = $"Mensagem de {item.Sender} excluída";
        UndoBar.IsVisible = true;
    }

    private void OnUndoClicked(object? sender, EventArgs e)
    {
        if (_lastDeleted is not { } last) return;

        Messages.Insert(Math.Min(last.Index, Messages.Count), last.Item);
        _lastDeleted = null;
        UndoBar.IsVisible = false;
    }
    // /snippet:list.styled.code

    public ListStyledPreview() => InitializeComponent();
}
