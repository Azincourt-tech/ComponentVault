using System.Globalization;
using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.DatePickers;

// snippet:control.daterangestrip
/// <summary>
/// Faixa de três semanas para escolher um intervalo sem abrir calendário. Pressione um dia e
/// arraste até outro, ou toque na ida e depois na volta. A faixa entre os dois é desenhada com
/// Microsoft.Maui.Graphics como uma pílula contínua por semana, que estica e encolhe (180ms) até
/// o dia sob o dedo. Os números são Labels por cima, transparentes ao toque.
/// </summary>
public class DateRangeStrip : ContentView, IDrawable
{
    private const int Days = 7;
    private const int Weeks = 3;
    private const float RowHeight = 44;
    private const string BandAnimation = "RangeBand";
    private const string PopAnimation = "RangePop";
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly GraphicsView _canvas;
    private readonly List<Label> _labels = [];
    private readonly DateTime _first;      // domingo da semana de hoje
    private readonly int _today;           // índice de hoje na grade (dias antes dele ficam bloqueados)
    private int? _start, _end;
    private int _anchor;
    private bool _awaitingEnd;             // ida escolhida com um toque: o próximo toque fecha a faixa
    private bool _closing;                 // este toque fechou a faixa; arrastar ajusta a volta
    private float _shownStart, _shownEnd;  // posições desenhadas (fracionárias durante a animação)
    private float _pop = 1;

    public event EventHandler? RangeChanged;

    public DateRangeStrip()
    {
        var today = DateTime.Today;
        _first = today.AddDays(-(int)today.DayOfWeek);
        _today = (today - _first).Days;

        var header = new Grid();
        var cells = new Grid { InputTransparent = true };
        for (var c = 0; c < Days; c++)
        {
            header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            cells.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var name = new Label
            {
                Text = PtBr.DateTimeFormat.AbbreviatedDayNames[c].TrimEnd('.'),
                FontFamily = "JakartaMedium",
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
            };
            name.SetThemeColor(Label.TextColorProperty, "InkTertiary");
            header.Add(name, c, 0);
        }

        for (var r = 0; r < Weeks; r++) cells.RowDefinitions.Add(new RowDefinition(RowHeight));
        for (var i = 0; i < Weeks * Days; i++)
        {
            var day = _first.AddDays(i);
            var label = new Label
            {
                Text = day.Day.ToString(PtBr),
                FontSize = 15,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            SemanticProperties.SetDescription(label, day.ToString("dddd, d 'de' MMMM", PtBr));
            _labels.Add(label);
            cells.Add(label, i % Days, i / Days);
        }

        _canvas = new GraphicsView { Drawable = this, HeightRequest = RowHeight * Weeks, BackgroundColor = Colors.Transparent };
        _canvas.StartInteraction += (_, e) => OnPress(e.Touches[0]);
        _canvas.DragInteraction += (_, e) => OnDrag(e.Touches[0]);
        _canvas.EndInteraction += (_, _) => OnRelease();
        _canvas.CancelInteraction += (_, _) => OnRelease();

        var month = new Label { Text = MonthTitle(), FontFamily = "JakartaSemiBold", FontSize = 15 };
        month.SetThemeColor(Label.TextColorProperty, "Ink");

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Children = { month, header, new Grid { Children = { _canvas, cells } } },
        };

        UpdateLabels();
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) =>
        {
            ThemeColors.PaletteChanged -= OnPaletteChanged;
            this.AbortAnimation(BandAnimation);
            this.AbortAnimation(PopAnimation);
        };
    }

    public DateTime? StartDate => _start is int s ? _first.AddDays(s) : null;

    /// <summary>Volta escolhida. Nulo enquanto só a ida foi tocada.</summary>
    public DateTime? EndDate => _end is int e && e != _start ? _first.AddDays(e) : null;

    /// <summary>Marca um intervalo pelo código (ex.: o valor salvo). Datas fora da faixa são ajustadas.</summary>
    public void SetRange(DateTime start, DateTime end)
    {
        var last = Weeks * Days - 1;
        var from = Math.Clamp((start.Date - _first).Days, _today, last);
        var to = Math.Clamp((end.Date - _first).Days, from, last);
        _awaitingEnd = from == to;
        Select(from, to);
    }

    private string MonthTitle()
    {
        var last = _first.AddDays(Weeks * Days - 1);
        var text = _first.Month == last.Month ? _first.ToString("MMMM 'de' yyyy", PtBr)
            : _first.Year == last.Year ? $"{_first.ToString("MMMM", PtBr)} – {last.ToString("MMMM 'de' yyyy", PtBr)}"
            : $"{_first.ToString("MMMM 'de' yyyy", PtBr)} – {last.ToString("MMMM 'de' yyyy", PtBr)}";
        return char.ToUpper(text[0], PtBr) + text[1..];
    }

    private int IndexAt(PointF point)
    {
        var width = _canvas.Width / Days;
        if (width <= 0) return _today;
        var col = Math.Clamp((int)(point.X / width), 0, Days - 1);
        var row = Math.Clamp((int)(point.Y / RowHeight), 0, Weeks - 1);
        return Math.Max(_today, (row * Days) + col);
    }

    private void OnPress(PointF point)
    {
        var index = IndexAt(point);
        if (_awaitingEnd && _start is int start && index > start)
        {
            _closing = true;
            Select(start, index);
            return;
        }

        _closing = false;
        _anchor = index;
        Select(index, index);
    }

    private void OnDrag(PointF point)
    {
        var index = IndexAt(point);
        if (_closing && _start is int start) Select(start, Math.Max(start, index));
        else Select(Math.Min(_anchor, index), Math.Max(_anchor, index));
    }

    // Soltou sem arrastar (ida e volta no mesmo dia): espera o toque da volta.
    private void OnRelease() => _awaitingEnd = !_closing && _start == _end;

    private void Select(int from, int to)
    {
        if (_start == from && _end == to) return;
        var single = from == to;
        _start = from;
        _end = to;
        UpdateLabels();

        this.AbortAnimation(BandAnimation);
        if (single || Handler is null)
        {
            // Dia novo (ou valor inicial): os círculos surgem com uma mola curta em vez de deslizar.
            _shownStart = from;
            _shownEnd = to;
            this.AbortAnimation(PopAnimation);
            this.Animate(PopAnimation, v => { _pop = (float)v; _canvas.Invalidate(); }, 0.5, 1, length: 220, easing: Easing.SpringOut);
        }
        else
        {
            float fromStart = _shownStart, fromEnd = _shownEnd;
            this.Animate(BandAnimation, t =>
            {
                _shownStart = fromStart + ((from - fromStart) * (float)t);
                _shownEnd = fromEnd + ((to - fromEnd) * (float)t);
                _canvas.Invalidate();
            }, length: 180, easing: Easing.CubicOut);
        }

        SemanticProperties.SetDescription(this, EndDate is DateTime end
            ? $"De {Day(StartDate!.Value)} a {Day(end)}"
            : $"Ida em {Day(StartDate!.Value)}, escolha a volta");
        RangeChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string Day(DateTime date) => date.ToString("d 'de' MMMM", PtBr);

    private void UpdateLabels()
    {
        for (var i = 0; i < _labels.Count; i++)
        {
            var label = _labels[i];
            var endpoint = i == _start || i == _end;
            var inside = _start is int s && _end is int e && i > s && i < e;
            label.FontFamily = endpoint || i == _today ? "JakartaSemiBold" : "JakartaRegular";
            label.SetThemeColor(Label.TextColorProperty,
                endpoint ? "OnAccent" : inside ? "AccentText" : i < _today ? "InkTertiary" : "Ink");
            label.Opacity = i < _today ? 0.5 : 1;
        }
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var width = rect.Width / Days;
        var accent = ThemeColors.Resolve("Accent", Colors.OrangeRed);

        if (_start is not null)
        {
            // Faixa: uma pílula por semana, cobrindo do primeiro ao último dia que cai nela.
            float lo = Math.Min(_shownStart, _shownEnd), hi = Math.Max(_shownStart, _shownEnd);
            canvas.FillColor = ThemeColors.Resolve("AccentTint", Colors.Bisque);
            for (var row = 0; row < Weeks; row++)
            {
                float rowStart = row * Days, rowEnd = rowStart + Days - 1;
                float a = Math.Max(lo, rowStart), b = Math.Min(hi, rowEnd);
                if (b < a) continue;
                float x0 = ((a - rowStart) * width) + 2, x1 = ((b - rowStart + 1) * width) - 2;
                canvas.FillRoundedRectangle(x0, (row * RowHeight) + 5, x1 - x0, RowHeight - 10, (RowHeight - 10) / 2);
            }

            // Ida e volta: círculos cheios na cor de destaque.
            canvas.FillColor = accent;
            var radius = ((Math.Min(width, RowHeight) / 2) - 3) * _pop;
            var ends = _end == _start ? new[] { _shownStart } : new[] { _shownStart, _shownEnd };
            foreach (var index in ends)
            {
                var center = Cell(index);
                canvas.FillCircle(center.X, center.Y, radius);
            }
        }

        // Hoje: um ponto embaixo do número (branco quando hoje é a ida ou a volta).
        var today = Cell(_today);
        canvas.FillColor = _today == _start || _today == _end ? Colors.White : accent;
        canvas.FillCircle(today.X, today.Y + 12, 2);

        PointF Cell(float index)
        {
            var row = Math.Clamp((int)Math.Floor(index / Days), 0, Weeks - 1);
            return new PointF((index - (row * Days) + 0.5f) * width, (row * RowHeight) + (RowHeight / 2));
        }
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => _canvas.Invalidate();
}
// /snippet:control.daterangestrip
