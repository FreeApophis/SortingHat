using System;

namespace SortingHat.API.AutoTag;

internal interface IDateTagPart
{
    string Key { get; }

    string Select(DateTime dateTime);
}