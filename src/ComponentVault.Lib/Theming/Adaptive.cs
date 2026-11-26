using System.ComponentModel;
using System.Globalization;

namespace ComponentVault.Lib.Theming;

/// <summary>Faixa de largura da janela, como os breakpoints da web.</summary>
public enum WidthClass
{
    /// <summary>Até 600px: celular em pé.</summary>
    Compact,

    /// <summary>De 600 a 1000px: tablet, celular deitado, janela estreita no desktop.</summary>
    Medium,

    /// <summary>A partir de 1000px: desktop.</summary>
    Expanded,
}

/// <summary>
/// Largura atual da janela e a faixa dela. O layout responde à largura disponível, não ao tipo
/// de aparelho: uma janela estreita no Windows recebe o mesmo layout de um celular.
/// O App chama <see cref="Update"/> quando a janela muda de tamanho.
/// </summary>
public sealed class Adaptive : INotifyPropertyChanged
{
    public const double CompactMax = 600;
    public const double MediumMax = 1000;

    private Adaptive()
    {
        // Antes da primeira medição, um palpite pelo aparelho.
        Class = DeviceInfo.Idiom == DeviceIdiom.Phone ? WidthClass.Compact : WidthClass.Expanded;
    }

    public static Adaptive Current { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A faixa mudou (não dispara a cada pixel de redimensionamento).</summary>
    public event EventHandler? ClassChanged;

    public double Width { get; private set; }

    public WidthClass Class { get; private set; }

    public bool IsCompact => Class == WidthClass.Compact;

    public static WidthClass Classify(double width) =>
        width < CompactMax ? WidthClass.Compact : width < MediumMax ? WidthClass.Medium : WidthClass.Expanded;

    public void Update(double width)
    {
        if (width <= 0) return;
        Width = width;

        var next = Classify(width);
        if (next == Class) return;

        Class = next;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompact)));
        ClassChanged?.Invoke(this, EventArgs.Empty);
    }
}

// snippet:control.onwidth
/// <summary>
/// Valor por faixa de largura, no lugar do OnIdiom (que olha o tipo de aparelho):
/// <code>ColumnDefinitions="{theming:OnWidth Compact='*', Default='*,*'}"</code>
/// Muda ao vivo quando a janela é redimensionada. Medium usa Default se não for informado.
/// </summary>
[ContentProperty(nameof(Default))]
[RequireService([typeof(IProvideValueTarget)])]
public sealed class OnWidthExtension : IMarkupExtension<BindingBase>
{
    public object? Compact { get; set; }

    public object? Medium { get; set; }

    public object? Expanded { get; set; }

    public object? Default { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var property = target?.TargetProperty as BindableProperty;
        var type = property?.ReturnType ?? typeof(object);
        return new Binding(nameof(Adaptive.Class), source: Adaptive.Current,
            converter: new Picker(this, type, ConverterFor(property, type)));
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

    // O MAUI declara alguns conversores na propriedade e não no tipo (Grid.ColumnDefinitions e
    // RowDefinitions, por exemplo): procura primeiro na propriedade CLR, depois no tipo.
    private static TypeConverter ConverterFor(BindableProperty? property, Type type)
    {
        var attribute = property?.DeclaringType
            .GetProperty(property.PropertyName)?
            .GetCustomAttributes(typeof(TypeConverterAttribute), inherit: true)
            .OfType<TypeConverterAttribute>()
            .FirstOrDefault();
        if (attribute is not null && Type.GetType(attribute.ConverterTypeName) is { } converterType
            && Activator.CreateInstance(converterType) is TypeConverter fromProperty)
        {
            return fromProperty;
        }

        return TypeDescriptor.GetConverter(type);
    }

    private sealed class Picker(OnWidthExtension values, Type targetType, TypeConverter converter) : IValueConverter
    {
        public object? Convert(object? value, Type _, object? parameter, CultureInfo culture)
        {
            var chosen = value switch
            {
                WidthClass.Compact => values.Compact ?? values.Default,
                WidthClass.Medium => values.Medium ?? values.Default,
                _ => values.Expanded ?? values.Default,
            };
            return To(chosen);
        }

        public object? ConvertBack(object? value, Type _, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        // Valores do XAML chegam como texto: converte com o conversor da propriedade ou do tipo
        // (Thickness, ColumnDefinitionCollection, LayoutOptions, números, enums...). Sem cache,
        // porque coleções como ColumnDefinitions não podem ser compartilhadas entre Grids.
        private object? To(object? value)
        {
            if (value is null || targetType == typeof(object) || targetType.IsInstanceOfType(value)) return value;
            if (value is not string text) return value;
            if (targetType.IsEnum) return Enum.Parse(targetType, text, ignoreCase: true);
            return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromInvariantString(text) : value;
        }
    }
}
// /snippet:control.onwidth
