using System.Globalization;
using System.Windows;

namespace Trarizon.Library.Wpf.Converters;
public sealed class BooleanToVisibilityConverter : TypedConverter<bool, Visibility>
{
    public override Visibility Convert(bool source, object parameter, CultureInfo culture)
        =>source? Visibility.Visible : Visibility.Collapsed;

    public override bool ConvertBack(Visibility target, object parameter, CultureInfo culture)
        => target is Visibility.Visible;
}
