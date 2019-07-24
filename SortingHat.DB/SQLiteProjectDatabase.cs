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
    public sealed class SQLiteProjectDatabase : IProjectDatabase, ISQLiteDatabase, IDisposable
    {
        private readonly string _path;
        private readonly string _dbName;
        private readonly string _encryptionKey = "Encrypted";

        private SqliteConnection Connection { get; }

        private readonly Func<IFile> _file;
        public IFile File => _file();

        private readonly Func<ITag> _tag;
        public ITag Tag => _tag();

        public SQLiteProjectDatabase(Func<IFile> file, Func<ITag> tag, DatabaseSettings databaseSettings)
        {
            _file = file;
            _tag = tag;
            _path = databaseSettings.DBPath == "#USERDOC" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : databaseSettings.DBPath;
            _dbName = databaseSettings.DBName;

            Connection = new SqliteConnection($"Filename={DBFile()}");
            Connection.Open();

            ExecuteNonQuery("PRAGMA foreign_keys=ON;");

            if (_encryptionKey != null)
            {
                ExecuteNonQuery("PRAGMA key='{_encryptionKey}';");
            }
        }

        private SqliteCommand CreateCommand(string commandText, params SqliteParameter[] parameters)
        {
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = commandText;

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }

        public void ExecuteNonQuery(string commandText, params SqliteParameter[] parameters)
        {
            CreateCommand(commandText, parameters).ExecuteNonQuery();
        }

        public object ExecuteScalar(string commandText, params SqliteParameter[] parameters)
        {
            return CreateCommand(commandText, parameters).ExecuteScalar();
        }

        public SqliteDataReader ExecuteReader(string commandText, params SqliteParameter[] parameters)
        {
            return CreateCommand(commandText, parameters).ExecuteReader();
        }

        public void Setup()
        {
            var migrator = new RevisionMigrator(this);

            migrator.Initialize();
            migrator.Migrate();
        }

        public void TearDown()
        {
            throw new NotImplementedException();
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

        private string DBPath()
        {
            string result = Path.Combine(_path, ".hat");

            if (Directory.Exists(result) == false)
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        private string DBFile()
        {
            return Path.Combine(DBPath(), $"{_dbName}.db");
        }

        #region IDisposable Support
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Connection.Close();
                    Connection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }


        #endregion
    }
}
