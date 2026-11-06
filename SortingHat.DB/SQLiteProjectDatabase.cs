using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB;

[UsedImplicitly]
public sealed class SQLiteProjectDatabase(DatabaseSettings databaseSettings, ISettings settings) : SQLiteDatabase(databaseSettings, settings.ActiveProject(databaseSettings)), IProjectDatabase
{
    internal override MigrationType MigrationType => MigrationType.Project;

    public Dictionary<string, long> GetStatistics()
    {
        return ExecuteReader(StatisticsQuery(), TransformStatistics);
    }

    private static string PerTableStatisticsQuery(string table)
    {
        return $"SELECT '{table}', Count(Id) FROM {table}";
    }

    private static Dictionary<string, long> TransformStatistics(SqliteDataReader reader)
    {
        var statistics = new Dictionary<string, long>();

        while (reader.Read())
        {
            ReadStatisticsLine(reader, statistics);
        }

        return statistics;
    }

    private static void ReadStatisticsLine(SqliteDataReader reader, Dictionary<string, long> statistics)
    {
        statistics[reader.GetString(0)] = reader.GetInt64(1);
    }

    private string StatisticsQuery()
    {
        var tables = new List<string> { "Files", "FilePaths", "FileNames", "Tags", "FileTags" };

        return string.Join(" UNION ", tables.Select(PerTableStatisticsQuery));
    }
}
