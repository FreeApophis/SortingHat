using System;

namespace SortingHat.API.AutoTag
{
    public class DayPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Day);
        public string Select(DateTime dateTime) => string.Format("{0}", dateTime.Day);
    }
}