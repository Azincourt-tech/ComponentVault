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
