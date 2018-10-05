using System;

namespace SortingHat.API.AutoTag
{
    public class DayPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Day);
        public string Select(DateTime dateTime) => $"{dateTime.Day}";
    }
}