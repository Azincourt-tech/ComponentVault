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

/// <summary>Mensagem da caixa de entrada do exemplo de puxar para atualizar.</summary>
public sealed record InboxMessage(string Initials, string Name, string Preview, string Time);

[ShowcasePreview("list.syncfusion")]
public partial class ListSyncfusionPreview : ContentView
{
    private readonly ObservableCollection<InboxMessage> _messages =
    [
        new("AS", "Ana Souza", "Revisão do contrato com os ajustes da cláusula 4", "09:42"),
        new("BL", "Bruno Lima", "Fotos do evento de sábado", "Ontem"),
        new("CM", "Carla Mendes", "Orçamento aprovado, podemos seguir", "Seg"),
        new("DR", "Diego Rocha", "Reunião remarcada para quinta", "Dom"),
    ];

    // Cada atualização traz uma das mensagens novas, em ordem.
    private readonly Queue<InboxMessage> _incoming = new(
    [
        new("EF", "Elisa Fontes", "Subi a nova versão do ícone", "Agora"),
        new("GP", "Gabriel Prado", "Os testes passaram no Android", "Agora"),
        new("HT", "Helena Tavares", "Pode revisar o texto da Home?", "Agora"),
    ]);

    public ListSyncfusionPreview()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(Inbox, _messages);
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        if (!Refresher.IsRefreshing) Refresher.IsRefreshing = true;
        OnRefreshing(Refresher, EventArgs.Empty);
    }

    // Simula a busca no servidor; num app real, aqui vai o RefreshCommand da ViewModel.
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Buscando mensagens novas…";
        await Task.Delay(900);
        if (_incoming.TryDequeue(out var message))
        {
            _messages.Insert(0, message);
            StatusLabel.Text = "1 mensagem nova";
        }
        else
        {
            StatusLabel.Text = "Nenhuma mensagem nova";
        }
        Refresher.IsRefreshing = false;
    }
}

[ShowcasePreview("list.custom")]
public partial class ListCustomPreview : ContentView
{
    private static readonly InboxItem[] Seed =
    [
        new("Ana Souza", "Revisão do contrato com os ajustes da cláusula 4", "09:42"),
        new("Bruno Lima", "Fotos do evento de sábado", "Ontem"),
        new("Carla Mendes", "Orçamento aprovado, podemos seguir", "Seg"),
        new("Diego Rocha", "Reunião remarcada para quinta", "Dom"),
    ];

    private readonly ObservableCollection<InboxItem> _messages = new(Seed);
    private (InboxItem Item, int Index)? _last;

    public ListCustomPreview()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(Inbox, _messages);
        UpdateCount();
    }

    private void OnSwiped(object? sender, SwipeAction action)
    {
        if ((sender as BindableObject)?.BindingContext is not InboxItem item) return;
        var index = _messages.IndexOf(item);
        if (index < 0) return;

        _messages.RemoveAt(index);
        _last = (item, index);
        UndoLabel.Text = $"Mensagem de {item.Sender} {(action == SwipeAction.Archive ? "arquivada" : "excluída")}";
        UndoBar.IsVisible = true;
        UpdateCount();
    }

    private void OnUndoClicked(object? sender, EventArgs e)
    {
        if (_last is not { } last) return;
        _messages.Insert(Math.Min(last.Index, _messages.Count), last.Item);
        _last = null;
        UndoBar.IsVisible = false;
        UpdateCount();
    }

    private void OnRestoreClicked(object? sender, EventArgs e)
    {
        _messages.Clear();
        foreach (var item in Seed) _messages.Add(item);
        _last = null;
        UndoBar.IsVisible = false;
        UpdateCount();
    }

    private void UpdateCount()
    {
        CountLabel.Text = _messages.Count == 1 ? "1 mensagem" : $"{_messages.Count} mensagens";
        EmptyState.IsVisible = _messages.Count == 0;
    }
}
