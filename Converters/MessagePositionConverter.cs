using System.Globalization;
namespace MarketPlace.Converters;
public class MessagePositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var message = value as Message;
        return message != null && message.IsSentByCurrentUser ? 2 : 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
