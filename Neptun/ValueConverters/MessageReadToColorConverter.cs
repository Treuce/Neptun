using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Neptun
{
    /// <summary>
    /// A converter that takes in a <see cref="MessageEntry.isRead"/> boolean value and returns a background color
    /// </summary>
    public class MessageReadToColorConverter : BaseValueConverter<MessageReadToColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
                return new SolidColorBrush((Color)Application.Current.TryFindResource("BackgroundDark_2"));
            else return new SolidColorBrush(Colors.Transparent);
		}

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
