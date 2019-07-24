using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace SortingHat.DB
{
    [UsedImplicitly]
    public sealed class SQLiteProjectDatabase : SQLiteDatabase, IProjectDatabase
    {

        private readonly Func<IFile> _file;
        public IFile File => _file();

        private readonly Func<ITag> _tag;
        public ITag Tag => _tag();

        public SQLiteProjectDatabase(Func<IFile> file, Func<ITag> tag, DatabaseSettings databaseSettings, string projectName) :
            base(databaseSettings, projectName)
        {
            _file = file;
            _tag = tag;
        }

        private static string PerTableStatisticsQuery(string table)
        {
            return $"SELECT '{table}', Count(ID) FROM {table}";
        }

        private string StatisticsQuery()
        {
            var tables = new List<string> { "Files", "FilePaths", "FileNames", "Tags", "FileTags" };

            return string.Join(" UNION ", tables.Select(PerTableStatisticsQuery));
        }

        public Dictionary<string, long> GetStatistics()
        {
            return TransformStatistics(ExecuteReader(StatisticsQuery()));
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

        internal override MigrationType MigrationType => MigrationType.Project;
    }
}
