using System;
using System.Globalization;

namespace SortingHat.API.AutoTag;

internal class HourPart : IDateTagPart
{
    public string Key => nameof(DateTime.Hour);

    public string Select(DateTime dateTime) => dateTime.Hour.ToString("D2", CultureInfo.InvariantCulture);
}