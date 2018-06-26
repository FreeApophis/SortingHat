using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace SortingHat.API
{
    public class RevisionMigrator
    {
        private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] TIME DEFAULT CURRENT_TIME NOT NULL);";
        private string _tableExists = string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';", "Revisions");

        public void Migrate()
        {
            using (var connection = DB.Connection())
            {
                foreach (var migration in Migrations())
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(migration))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            connection.Open();

                            SqliteCommand initializeCommand = connection.CreateCommand();
                            initializeCommand.CommandText = reader.ReadToEnd();
                            initializeCommand.ExecuteNonQuery();

                            connection.Close();
                        }
                    }
                }
            }
        }

        private IEnumerable<string> Migrations()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(res => res.StartsWith("SortingHat.API.Migrations"));
        }

        public void Initialize()
        {
            using (var connection = DB.Connection(DB.DBFile(Directory.GetCurrentDirectory())))
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
