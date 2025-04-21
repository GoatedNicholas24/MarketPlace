using System.Globalization;
namespace MarketPlace.Converters;

public class SellerOrderStatusToButtonsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value?.ToString()?.ToLower();
        var buttons = new List<string>();

        switch (status)
        {
            case "pending":
                buttons.Add("Accept");
                buttons.Add("Reject");
                break;

            case "processing":
                buttons.Add("Delivered");
                buttons.Add("Chat");
                break;

            case "sent":
                buttons.Add("Chat");
                
                break;

            case "received":
                buttons.Add("Chat");
                break;
            case "rejected":
                buttons.Add("Chat");
            
                break;
        }

        return buttons;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
