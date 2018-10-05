using System;

namespace SortingHat.API.AutoTag
{
    public class MonthPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Month);
        public string Select(DateTime dateTime) => $"{dateTime.Month}";
    }
}