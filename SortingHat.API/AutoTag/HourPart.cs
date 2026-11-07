using System;

namespace SortingHat.API.AutoTag;

internal class HourPart : IDateTagPart
{
    public string Key => nameof(DateTime.Hour);

    public string Select(DateTime dateTime) => $"{dateTime.Hour}";
}