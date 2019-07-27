using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SortingHat.API;

namespace SortingHat.DB
{
    public abstract class SQLiteDatabase : IDisposable
    {
        private readonly string _path;
        private readonly string _encryptionKey = "Encrypted";

        private SqliteConnection Connection { get; }

        protected SQLiteDatabase(DatabaseSettings databaseSettings, string dbName)
        {
            if (databaseSettings.Type == "sqlite")
            {
                _path = databaseSettings.DbPath;
                DbName = dbName;

                Connection = new SqliteConnection($"Filename={DbFile()}");
                Connection.Open();

                ExecuteNonQuery("PRAGMA foreign_keys=ON;");

                if (_encryptionKey != null)
                {
                    ExecuteNonQuery("PRAGMA key='{_encryptionKey}';");
                }
            } else
            {
                throw new NotSupportedException("Only SQLite is currently supported as Database Type");
            }
        }

        internal abstract MigrationType MigrationType { get; }

        public string DbName { get; }

        internal void ExecuteNonQuery(string commandText, params SqliteParameter[] parameters)
        {
            CreateCommand(commandText, parameters).ExecuteNonQuery();
        }

        internal object ExecuteScalar(string commandText, params SqliteParameter[] parameters)
        {
            return CreateCommand(commandText, parameters).ExecuteScalar();
        }

        internal SqliteDataReader ExecuteReader(string commandText, params SqliteParameter[] parameters)
        {
            return CreateCommand(commandText, parameters).ExecuteReader();
        }

        private string ExistingDbPath()
        {
            if (Directory.Exists(_path) == false)
            {
                Directory.CreateDirectory(_path);
            }

            return _path;
        }

        private string DbFile()
        {
            return Path.Combine(ExistingDbPath(), $"{DbName}.db");
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
    }
}
