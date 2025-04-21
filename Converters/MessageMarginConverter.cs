using System.Globalization;
namespace MarketPlace.Converters;
public class MessageMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSentByCurrentUser)
        {
            return isSentByCurrentUser ? new Thickness(50, 0, 0, 0) : new Thickness(0, 0, 50, 0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 