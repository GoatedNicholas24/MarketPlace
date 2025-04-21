using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MarketPlace.Converters
{
    public class DateTimeToPrettyDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DateTimeOffset dto)
                    return dto.LocalDateTime.ToString("dddd, dd MMMM yyyy - hh:mm tt");

                if (value is DateTime dt)
                    return dt.ToLocalTime().ToString("dddd, dd MMMM yyyy - hh:mm tt");

                if (value is string s && DateTime.TryParse(s, out var parsedDate))
                    return parsedDate.ToLocalTime().ToString("dddd, dd MMMM yyyy - hh:mm tt");

                return "Unknown date";
            }
            catch
            {
                return "Invalid date";
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
