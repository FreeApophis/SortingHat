using System.Collections.Generic;

namespace SortingHat.API.DI;

public interface IDatabase
{
    /// <summary>The number of rows in each table of the database, by table name.</summary>
    IReadOnlyDictionary<string, long> GetStatistics();
}
