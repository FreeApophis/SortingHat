using System.Collections.Generic;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB;

/// <summary>The database of the active project, decided on its first use, so a command that never uses it does not even read the settings.</summary>
internal sealed class SQLiteProjectDatabase(DatabaseSettings databaseSettings, ISettings settings, IRevisionMigrator migrator)
    : SQLiteDatabase(databaseSettings, () => settings.ActiveProject(databaseSettings), migrator), IProjectDatabase
{
    internal override MigrationType MigrationType => MigrationType.Project;

    public IReadOnlyDictionary<string, long> GetStatistics()
        => CountRows(["Files", "FilePaths", "FileNames", "Tags", "FileTags"]);
}
