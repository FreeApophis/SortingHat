using System;

namespace SortingHat.API.AutoTag
{
    public interface IDateTagPart
    {
        string Key { get; }
        string Select(DateTime dateTime);
    }
}