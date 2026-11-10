using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SortingHat.Desktop.Views;

/// <summary>Shows a UTC time of the database as a short local date and time; the conversion to local time belongs to the view, not the pure core.</summary>
internal sealed class LocalDateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dateTime ? dateTime.ToLocalTime().ToString("g", culture) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("The date is only shown.");
}
