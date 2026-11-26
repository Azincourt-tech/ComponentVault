using System.ComponentModel;
using System.Globalization;

namespace ComponentVault.Lib.Showcase.DatePickers;

[ShowcasePreview("datepicker.base")]
public partial class DatePickerBasePreview : ContentView
{
    public DatePickerBasePreview() => InitializeComponent();
}

[ShowcasePreview("datepicker.styled")]
public partial class DatePickerStyledPreview : ContentView
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public DatePickerStyledPreview()
    {
        InitializeComponent();

        // Não faz sentido agendar no passado: o mínimo é hoje; o padrão, amanhã às 9h.
        DateInput.MinimumDate = DateTime.Today;
        DateInput.Date = DateTime.Today.AddDays(1);
        TimeInput.Time = new TimeSpan(9, 0, 0);
        UpdateSummary();
    }

    private void OnDateChanged(object? sender, DateChangedEventArgs e) => UpdateSummary();

    private void OnTimeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time)) UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (DateInput.Date is not { } date || TimeInput.Time is not { } time)
        {
            SummaryLabel.Text = "Escolha data e horário.";
            return;
        }

        var when = date.Date + time;
        var day = (when.Date - DateTime.Today).Days switch
        {
            0 => "hoje",
            1 => "amanhã",
            _ => when.ToString("dddd, d 'de' MMMM", PtBr),
        };
        SummaryLabel.Text = $"Você será lembrado {day} às {when:HH:mm}.";
    }
}

[ShowcasePreview("datepicker.uranium")]
public partial class DatePickerUraniumPreview : ContentView
{
    public DatePickerUraniumPreview() => InitializeComponent();
}

[ShowcasePreview("datepicker.syncfusion")]
public partial class DatePickerSyncfusionPreview : ContentView
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public DatePickerSyncfusionPreview()
    {
        InitializeComponent();
        // Abre com uma semana já escolhida, para o intervalo aparecer de cara.
        var start = DateTime.Today.AddDays(7);
        TripCalendar.SelectedDateRange = new Syncfusion.Maui.Toolkit.Calendar.CalendarDateRange(start, start.AddDays(6));
        TripCalendar.DisplayDate = start;
        UpdateLabel();
    }

    private void OnSelectionChanged(object? sender, EventArgs e) => UpdateLabel();

    // Resumo em frase: "14 dez → 20 dez · 6 noites", ou o que falta escolher.
    private void UpdateLabel()
    {
        var range = TripCalendar.SelectedDateRange;
        RangeLabel.Text = range?.StartDate is not DateTime from ? "Escolha a ida"
            : range.EndDate is not DateTime to ? $"Ida em {from.ToString("d MMM", PtBr)}. Escolha a volta"
            : $"{from.ToString("d MMM", PtBr)} → {to.ToString("d MMM", PtBr)} · {(to - from).Days} noites";
    }
}

/// <summary>Compromisso da agenda do exemplo com Plugin.Maui.Calendar.</summary>
public sealed record AgendaItem(string Time, string Title);

[ShowcasePreview("datepicker.calendar")]
public partial class DatePickerCalendarPreview : ContentView
{
    public DatePickerCalendarPreview()
    {
        InitializeComponent();
        // Compromissos espalhados pelo mês, a partir de hoje (num app real, viriam do serviço de agenda).
        var today = DateTime.Today;
        Agenda.Events = new Plugin.Maui.Calendar.Models.EventCollection
        {
            [today] = new List<AgendaItem> { new("09:00", "Daily do time"), new("14:30", "Revisão do Slider") },
            [today.AddDays(2)] = new List<AgendaItem> { new("10:00", "Entrevista de design") },
            [today.AddDays(5)] = new List<AgendaItem> { new("16:00", "Demo para o cliente") },
            [today.AddDays(9)] = new List<AgendaItem> { new("11:00", "Planejamento da sprint"), new("15:00", "1:1 com a Ana") },
            [today.AddDays(-3)] = new List<AgendaItem> { new("13:00", "Retrospectiva") },
        };
        Agenda.Culture = CultureInfo.GetCultureInfo("pt-BR"); // CultureInfo: o XAML não converte a partir de texto
        Agenda.SelectedDate = today;
    }
}

[ShowcasePreview("datepicker.custom")]
public partial class DatePickerCustomPreview : ContentView
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public DatePickerCustomPreview()
    {
        InitializeComponent();
        // Abre com um fim de semana prolongado já marcado, para a faixa aparecer de cara.
        Strip.SetRange(DateTime.Today.AddDays(3), DateTime.Today.AddDays(6));
        OnRangeChanged(Strip, EventArgs.Empty);
    }

    private void OnRangeChanged(object? sender, EventArgs e)
    {
        if (SummaryLabel is null) return;
        SummaryLabel.Text = Strip.StartDate is not DateTime from ? "Escolha a entrada"
            : Strip.EndDate is not DateTime to ? $"Entrada em {from.ToString("d MMM", PtBr)}. Toque na saída"
            : $"{from.ToString("ddd, d MMM", PtBr)} → {to.ToString("ddd, d MMM", PtBr)} · {(to - from).Days} {((to - from).Days == 1 ? "noite" : "noites")}";
    }
}
