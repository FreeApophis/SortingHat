using System;

namespace SortingHat.API.AutoTag
{
    public class YearPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Year);
        public string Select(DateTime dateTime) => $"{dateTime.Year}";
    }
}