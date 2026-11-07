using System;

namespace SortingHat.API.AutoTag;

internal class SecondPart : IDateTagPart
{
    public string Key => nameof(DateTime.Second);

    public string Select(DateTime dateTime) => $"{dateTime.Second}";
}