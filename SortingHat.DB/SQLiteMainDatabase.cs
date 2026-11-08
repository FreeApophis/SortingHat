using System.Collections.Generic;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB;

/// <summary>The database of hat itself: the projects and the settings.</summary>
internal sealed class SQLiteMainDatabase(DatabaseSettings databaseSettings, IRevisionMigrator migrator)
    : SQLiteDatabase(databaseSettings, () => databaseSettings.Name, migrator), IMainDatabase
{
    internal override MigrationType MigrationType => MigrationType.Main;

    public IReadOnlyDictionary<string, long> GetStatistics()
        => CountRows(["Projects", "Settings"]);
}
