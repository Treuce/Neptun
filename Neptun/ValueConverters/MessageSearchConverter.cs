using System;
using System.Globalization;
using System.Windows;

namespace Neptun
{
    /// <summary>
    /// A converter that takes in a boolean and inverts it
    /// </summary>
    public class MessageSearchConverter : BaseValueConverter<MessageSearchConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
		}

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (MessageSearchEnum)value;
        }
    }
}
