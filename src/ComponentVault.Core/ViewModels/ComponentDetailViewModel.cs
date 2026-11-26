using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Navigation;
using ComponentVault.Core.Presentation;
using ComponentVault.Core.Services.Highlighting;
using ComponentVault.Core.Services.Metadata;
using ComponentVault.Core.Services.Snippets;

namespace ComponentVault.Core.ViewModels;

public sealed partial class ComponentDetailViewModel : BaseViewModel, INavigationAware
{
    public static readonly TimeSpan CopyFeedbackDuration = TimeSpan.FromSeconds(2);

    private readonly IComponentMetadataService _metadata;
    private readonly ISnippetRepository _snippets;
    private readonly ISyntaxHighlighter _highlighter;
    private readonly IClipboardService _clipboard;
    private readonly TimeProvider _time;
    private CancellationTokenSource? _feedbackCts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle), nameof(Docs), nameof(IsLoaded))]
    private ComponentItem? _component;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCompare), nameof(CompareVariants))]
    private IReadOnlyList<VariantOption> _variants = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositionText))]
    private VariantOption? _selectedVariant;

    [ObservableProperty]
    private IReadOnlyList<CodeSnippetModel> _snippetModels = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPreviewTab), nameof(IsCompareTab), nameof(IsDocsTab), nameof(IsCodeTab), nameof(CompareVariants))]
    private DetailTab _selectedTab = DetailTab.Preview;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCopyFeedback))]
    private string? _copyFeedback;

    [ObservableProperty]
    private bool _isPresentationMode;

    public ComponentDetailViewModel(
        IComponentMetadataService metadata,
        ISnippetRepository snippets,
        ISyntaxHighlighter highlighter,
        IClipboardService clipboard,
        TimeProvider time)
    {
        _metadata = metadata;
        _snippets = snippets;
        _highlighter = highlighter;
        _clipboard = clipboard;
        _time = time;
    }

    public bool IsLoaded => Component is not null;

    public string Subtitle => Component is null
        ? string.Empty
        : $"{Labels.Category(Component.Category)} · {Labels.Count(Component.Variants.Count, "nível", "níveis")}";

    public ComponentDocs? Docs => Component?.Docs;

    public bool IsPreviewTab => SelectedTab == DetailTab.Preview;

    public bool IsCompareTab => SelectedTab == DetailTab.Compare;

    /// <summary>A comparação só faz sentido com dois níveis ou mais.</summary>
    public bool CanCompare => Variants.Count > 1;

    /// <summary>Variantes da aba Lado a lado; vazia fora dela, para não montar previews escondidos.</summary>
    public IReadOnlyList<VariantOption> CompareVariants => IsCompareTab ? Variants : [];

    public bool IsDocsTab => SelectedTab == DetailTab.Docs;

    public bool IsCodeTab => SelectedTab == DetailTab.Code;

    public bool HasCopyFeedback => CopyFeedback is not null;

    /// <summary>Posição do nível na apresentação, ex.: "2 de 3".</summary>
    public string PositionText => SelectedVariant is null
        ? string.Empty
        : $"{IndexOf(SelectedVariant) + 1} de {Variants.Count}";

    public void OnNavigatedTo(IReadOnlyDictionary<string, object> parameters)
    {
        var id = parameters.TryGetValue(Routes.Params.ComponentId, out var value) ? value?.ToString() : null;
        var component = id is null ? null : _metadata.GetById(id);
        if (component is null)
        {
            ErrorMessage = $"Componente '{id}' não encontrado.";
            return;
        }

        ErrorMessage = null;
        Component = component;
        Title = component.Name;
        Variants = component.Variants.Select(v => new VariantOption(v)).ToArray();

        var requested = parameters.TryGetValue(Routes.Params.VariantId, out var variantId) ? variantId?.ToString() : null;
        SelectVariant(Variants.FirstOrDefault(v => v.Variant.Id == requested) ?? Variants[0]);

        if (parameters.TryGetValue(Routes.Params.Present, out var present)
            && bool.TryParse(present?.ToString(), out var presenting) && presenting)
        {
            SelectedTab = DetailTab.Preview;
            IsPresentationMode = true;
        }
    }

    [RelayCommand]
    private void SelectVariant(VariantOption? option)
    {
        if (option is null) return;

        foreach (var variant in Variants) variant.IsSelected = ReferenceEquals(variant, option);
        SelectedVariant = option;
        SnippetModels = option.Variant.Snippets.Select(BuildSnippet).ToArray();
    }

    [RelayCommand]
    private void SelectTab(DetailTab tab) => SelectedTab = tab;

    /// <summary>RF10: esconde o entorno e mostra só o componente. Entrar sempre leva à aba Preview.</summary>
    [RelayCommand]
    private void TogglePresentation()
    {
        IsPresentationMode = !IsPresentationMode;
        if (IsPresentationMode) SelectedTab = DetailTab.Preview;
    }

    [RelayCommand]
    private void ExitPresentation() => IsPresentationMode = false;

    /// <summary>Próximo nível, voltando ao primeiro depois do último (setas, gesto, apresentação).</summary>
    [RelayCommand]
    private void NextVariant() => Step(+1);

    [RelayCommand]
    private void PreviousVariant() => Step(-1);

    private void Step(int direction)
    {
        if (Variants.Count < 2 || SelectedVariant is null) return;
        var index = (IndexOf(SelectedVariant) + direction + Variants.Count) % Variants.Count;
        SelectVariant(Variants[index]);
    }

    private int IndexOf(VariantOption option)
    {
        for (var i = 0; i < Variants.Count; i++)
        {
            if (ReferenceEquals(Variants[i], option)) return i;
        }
        return 0;
    }

    [RelayCommand]
    private async Task CopySnippetAsync(CodeSnippetModel? snippet)
    {
        if (snippet is null || snippet.IsMissing) return;

        await _clipboard.SetTextAsync(snippet.Code);
        await ShowCopyFeedbackAsync($"{snippet.Title} copiado para a área de transferência");
    }

    private async Task ShowCopyFeedbackAsync(string message)
    {
        // Um novo toque reinicia o tempo em vez de empilhar avisos.
        _feedbackCts?.Cancel();
        var cts = _feedbackCts = new CancellationTokenSource();
        CopyFeedback = message;

        try
        {
            await Task.Delay(CopyFeedbackDuration, _time, cts.Token);
            CopyFeedback = null;
        }
        catch (OperationCanceledException)
        {
            // Substituído por um aviso mais novo.
        }
    }

    private CodeSnippetModel BuildSnippet(CodeSnippetRef reference)
    {
        var code = _snippets.Find(reference.SnippetId);
        return code is null
            ? CodeSnippetModel.Missing(reference)
            : new CodeSnippetModel(reference.Title, reference.Language, code, _highlighter.Tokenize(code, reference.Language));
    }
}

public sealed partial class VariantOption(ComponentVariant variant) : ObservableObject
{
    public ComponentVariant Variant { get; } = variant;

    public string Id => Variant.Id;

    public string Title => Variant.Title;

    public string Summary => Variant.Summary;

    public ComponentOrigin Origin => Variant.Origin;

    public string TierText => Labels.Tier(Variant.Tier);

    public string? PackageId => Variant.PackageId;

    public IReadOnlyList<string> Highlights => Variant.Highlights;

    public bool HasHighlights => Variant.Highlights.Count > 0;

    [ObservableProperty]
    private bool _isSelected;
}

public sealed record CodeSnippetModel(string Title, CodeLanguage Language, string Code, IReadOnlyList<SyntaxToken> Tokens)
{
    public bool IsMissing { get; private init; }

    public string LanguageText => Language == CodeLanguage.Xaml ? "XAML" : "C#";

    public static CodeSnippetModel Missing(CodeSnippetRef reference) =>
        new(reference.Title, reference.Language, $"Snippet '{reference.SnippetId}' não encontrado no código-fonte.", [])
        {
            IsMissing = true,
        };
}
