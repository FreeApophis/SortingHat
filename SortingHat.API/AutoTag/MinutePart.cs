using System;
using System.Globalization;

namespace SortingHat.API.AutoTag;

internal class MinutePart : IDateTagPart
{
    public string Key => nameof(DateTime.Minute);

    public string Select(DateTime dateTime) => dateTime.Minute.ToString("D2", CultureInfo.InvariantCulture);
}