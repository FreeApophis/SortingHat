using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortingHat.API.AutoTag
{
    public abstract class DateAutoTag : IAutoTag
    {
        private readonly List<IDateTagPart> _selectableDateParts = new List<IDateTagPart> { new YearPart(), new MonthPart(), new DayPart(), new WeekDayPart(), new HourPart(), new MinutePart(), new SecondPart() };

        public abstract string AutoTagKey { get; }
        public abstract string Description { get; }
        public string HumanReadableAutoTagsKey =>
            AutoTagKey.Replace("<>", "<DatePart> Possible values: " + string.Join(", ", _selectableDateParts.Select(part => part.Key)));

        public string HandleTag(FileInfo file, string tagMatch)
        {
            return HandleTag(file, _selectableDateParts.First(part => part.Key == tagMatch));
        }

        protected abstract string HandleTag(FileInfo file, IDateTagPart tagPart);

        public string FindMatch(string value)
        {
            var findString = new Regex(AutoTagKey.Replace("<>", @"(\w+)"));
            var match = findString.Match(value);

            if (!match.Success)
            {
                return null;
            }

            return _selectableDateParts
                .Select(parts => parts.Key)
                .FirstOrDefault(parts => parts == match.Groups[1].Value);
        }
    }
}
