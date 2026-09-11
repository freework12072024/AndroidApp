using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSolution.Converters
{
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        {
            return value is bool isMine && isMine
                ? LayoutOptions.End
                : LayoutOptions.Start;
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
