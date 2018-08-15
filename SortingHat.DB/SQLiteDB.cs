using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using System;
using System.IO;

namespace SortingHat.DB
{
    public class SQLiteDB : IDatabase, IDisposable
    {
        private readonly string _path;
        private readonly string _dbName;
        private readonly string _encryptionKey = "Encrypted";

        internal SqliteConnection Connection { get; }

        public IFile File { get; }
        public ITag Tag { get; }

        public SQLiteDB(DatabaseSettings databaseSettings)
        {
            _path = databaseSettings.DBPath == "#USERDOC" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : databaseSettings.DBPath;
            _dbName = databaseSettings.DBName;

            Connection = new SqliteConnection($"Filename={DBFile()}");
            Connection.Open();

            ExecuteNonQuery("PRAGMA foreign_keys=ON;");

            if (_encryptionKey != null)
            {
                ExecuteNonQuery("PRAGMA key='{_encryptionKey}';");
            }

            File = new SQLiteFile(this);
            Tag = new SQLiteTag(this);
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
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Connection.Close();
                    Connection.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
