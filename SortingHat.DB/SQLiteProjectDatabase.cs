using System.Collections.Generic;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB;

[UsedImplicitly]
public sealed class SQLiteProjectDatabase(DatabaseSettings databaseSettings, ISettings settings) : SQLiteDatabase(databaseSettings, settings.ActiveProject(databaseSettings)), IProjectDatabase
{
    internal override MigrationType MigrationType => MigrationType.Project;

    public IReadOnlyDictionary<string, long> GetStatistics()
        => CountRows(["Files", "FilePaths", "FileNames", "Tags", "FileTags"]);
}
