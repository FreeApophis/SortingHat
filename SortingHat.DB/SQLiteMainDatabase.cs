using System.Collections.Generic;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB;

public class SQLiteMainDatabase(DatabaseSettings databaseSettings) : SQLiteDatabase(databaseSettings, databaseSettings.Name), IMainDatabase
{
    internal override MigrationType MigrationType => MigrationType.Main;

    public Dictionary<string, long> GetStatistics()
    {
        throw new System.NotImplementedException();
    }
}
