using System;

namespace SortingHat.API.AutoTag
{
    public class HourPart : IDateTagPart
    {
        public string Key => nameof(DateTime.Hour);
        public string Select(DateTime dateTime) => string.Format("{0}", dateTime.Hour);
    }
}