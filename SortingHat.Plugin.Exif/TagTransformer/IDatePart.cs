using System;

namespace SortingHat.Plugin.Exif.TagTransformer
{
    internal interface IDatePart
    {
        string Select(DateTime dateTime);
    }

    internal class YearPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Year);
    }

    internal class MonthPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Month);
    }

    internal class DayPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Day);
    }

    internal class WeekDayPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.DayOfWeek.ToString());
    }

    internal class HourPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Hour);
    }

    internal class MinutePart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Minute);
    }

    internal class SecondPart : IDatePart
    {
        string IDatePart.Select(DateTime dateTime) => string.Format("{0}", dateTime.Second);
    }
}