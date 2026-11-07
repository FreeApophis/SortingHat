using System;
using System.Globalization;

namespace SortingHat.API.AutoTag;

internal class DayPart : IDateTagPart
{
    public string Key => nameof(DateTime.Day);

    public string Select(DateTime dateTime) => dateTime.Day.ToString("D2", CultureInfo.InvariantCulture);
}