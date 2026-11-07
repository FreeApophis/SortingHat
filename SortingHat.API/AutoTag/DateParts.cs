using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>The parts of a date a <see cref="DateAutoTag"/> can give.</summary>
internal static class DateParts
{
    private static readonly IReadOnlyList<IDateTagPart> All = [new YearPart(), new MonthPart(), new DayPart(), new WeekDayPart(), new HourPart(), new MinutePart(), new SecondPart()];

    public static AutoTagParameter Parameter { get; } = AutoTagParameter.OneOf([.. All.Select(part => part.Key)]);

    public static Option<IDateTagPart> Find(string key)
        => All.FirstOrNone(part => part.Key == key);
}
