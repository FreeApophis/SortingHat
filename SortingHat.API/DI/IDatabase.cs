using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IDatabase
    {
        Dictionary<string, long> GetStatistics();
    }
}