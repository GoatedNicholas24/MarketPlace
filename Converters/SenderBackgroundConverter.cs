using System.Globalization;
namespace MarketPlace.Converters;
public class SenderBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var message = value  as Message;
        var currentUser = Preferences.Get("UserEmail", "null");

        return message?.senderEmail == currentUser ? Colors.LightGreen : Colors.LightGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
