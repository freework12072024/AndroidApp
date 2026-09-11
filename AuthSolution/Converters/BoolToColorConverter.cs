using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSolution.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        {
            return value is bool isMine && isMine
                ? Color.FromArgb("#005C4B")
                : Color.FromArgb("#202C33");
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
