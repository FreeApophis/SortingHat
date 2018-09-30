using System;

namespace SortingHat.API.AutoTag
{
    public class YearPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Year);
        public string Select(DateTime dateTime) => string.Format("{0}", dateTime.Year);
    }
}