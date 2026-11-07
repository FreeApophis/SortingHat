using System;
using System.Globalization;

namespace SortingHat.API.AutoTag;

internal class MonthPart : IDateTagPart
{
    public string Key => nameof(DateTime.Month);

    public string Select(DateTime dateTime) => dateTime.Month.ToString("D2", CultureInfo.InvariantCulture);
}