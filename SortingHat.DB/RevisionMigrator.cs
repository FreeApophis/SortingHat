using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace SortingHat.DB
{
    class RevisionMigrator
    {
        private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";
        private readonly string _tableExists = string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';", "Revisions");
        private SQLiteDB _connection;

        public RevisionMigrator(SQLiteDB connection)
        {
            _connection = connection;
        }

        public void Migrate()
        {
            foreach (var migration in Migrations())
            {
                if (HasMigrated(migration))
                {
                    continue;
                }

                Migrate(migration);
            }
        }

        private void Migrate(string migration)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(migration))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    using (var connection = _connection.Connection())
                    {
                        connection.Open();

                        SqliteCommand initializeCommand = connection.CreateCommand();
                        initializeCommand.CommandText = reader.ReadToEnd();
                        initializeCommand.ExecuteNonQuery();

                        connection.Close();

                        SetMigrated(migration);
                    }
                }
            }
        }

        private void SetMigrated(string migration)
        {

            using (var connection = _connection.Connection())
            {
                connection.Open();

                SqliteCommand migratedCommand = connection.CreateCommand();
                migratedCommand.CommandText = $"INSERT INTO Revisions (Name, MigratedAt) VALUES ('{migration}',  datetime('now'))";
                migratedCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        private bool HasMigrated(string migration)
        {
            bool result = true;

            using (var connection = _connection.Connection())
            {
                connection.Open();

                SqliteCommand hasMigratedCommand = connection.CreateCommand();
                hasMigratedCommand.CommandText = $"SELECT ID From Revisions WHERE Name = '{migration}'";
                result = ((long?)hasMigratedCommand.ExecuteScalar()).HasValue;

                connection.Close();
            }

            return result;
        }

        private IEnumerable<string> Migrations()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(res => res.StartsWith("SortingHat.DB.Migrations"));
        }

        public void Initialize()
        {
            using (var connection = _connection.Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = CreateRevisionTableCommand;
                initializeCommand.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
