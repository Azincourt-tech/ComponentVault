using CommunityToolkit.Mvvm.ComponentModel;

namespace ComponentVault.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    public bool HasError => ErrorMessage is not null;

    /// <summary>Executa com IsBusy ligado e converte exceções em ErrorMessage, sem derrubar a tela.</summary>
    protected async Task RunBusyAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = errorMessage ?? ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
