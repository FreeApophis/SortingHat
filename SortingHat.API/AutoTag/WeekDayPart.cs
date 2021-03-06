﻿using System;

namespace SortingHat.API.AutoTag
{
    public class WeekDayPart : IDateTagPart
    {
        public string Key => nameof(DateTime.DayOfWeek);
        public string Select(DateTime dateTime) => $"{dateTime.DayOfWeek.ToString()}";
    }
}