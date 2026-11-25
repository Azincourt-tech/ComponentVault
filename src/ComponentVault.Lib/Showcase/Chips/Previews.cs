using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComponentVault.Lib.Showcase.Chips;

/// <summary>Item de chip para a demonstração (num app real viria da ViewModel).</summary>
public sealed class ChipItem(string text, bool isSelected = false) : INotifyPropertyChanged
{
    private bool _isSelected = isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Text { get; } = text;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

[ShowcasePreview("chips.styled")]
public partial class ChipsStyledPreview : ContentView
{
    public ChipsStyledPreview()
    {
        InitializeComponent();
        foreach (var chip in FilterChips) chip.PropertyChanged += (_, _) => OnPropertyChanged(nameof(FilterSummary));
        BindingContext = this;
    }

    public ObservableCollection<ChipItem> ChoiceChips { get; } =
        [new("Todos", true), new("Nativos"), new("Custom"), new("Bibliotecas")];

    public ObservableCollection<ChipItem> FilterChips { get; } =
        [new("Android", true), new("Windows", true), new("iOS"), new("Tema escuro")];

    public ObservableCollection<ChipItem> InputChips { get; } =
        [new("maui"), new("mvvm"), new("design-system")];

    public string FilterSummary
    {
        get
        {
            var selected = FilterChips.Where(c => c.IsSelected).Select(c => c.Text).ToArray();
            return selected.Length == 0 ? "Nenhum filtro: mostrando tudo" : $"Filtrando por {string.Join(", ", selected)}";
        }
    }

    private void OnChoiceTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not ChipItem tapped) return;
        foreach (var chip in ChoiceChips) chip.IsSelected = ReferenceEquals(chip, tapped);
    }

    private void OnFilterTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is ChipItem chip) chip.IsSelected = !chip.IsSelected;
    }

    private void OnRemoveTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is ChipItem chip) InputChips.Remove(chip);
    }

    private void OnTagCompleted(object? sender, EventArgs e)
    {
        var text = TagEntry.Text?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(text) || InputChips.Any(c => c.Text == text)) return;

        InputChips.Add(new ChipItem(text));
        TagEntry.Text = string.Empty;
    }
}
