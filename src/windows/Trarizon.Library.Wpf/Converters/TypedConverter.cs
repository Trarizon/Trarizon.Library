using System.Globalization;
using System.Windows.Data;

namespace Trarizon.Library.Wpf.Converters;
public abstract class TypedConverter<TSource, TTarget> : IValueConverter
{
#nullable disable
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var src = (TSource)value;
        return Convert(src, parameter, culture);
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var tgt = (TTarget)value;
        return ConvertBack(tgt, parameter, culture);
    }
#nullable enable

    public abstract TTarget Convert(TSource source, object parameter, CultureInfo culture);
    public abstract TSource ConvertBack(TTarget target, object parameter, CultureInfo culture);
}
