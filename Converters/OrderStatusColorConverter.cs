using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Globalization;
namespace MarketPlace.Converters;
public class OrderStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToString() switch
            {
                "received" => Colors.Green,
                "cancelled" => Colors.Red,
                "cancelled_by_user"=>Colors.Red,
                "pending" => Colors.Grey,
               
                _ => Colors.LightBlue
            };
        }
        return Colors.Black; // Default color
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
