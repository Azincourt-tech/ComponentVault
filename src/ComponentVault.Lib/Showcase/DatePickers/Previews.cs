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
