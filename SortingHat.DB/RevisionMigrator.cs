using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SortingHat.DB
{
    class RevisionMigrator
    {
        private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";
        private readonly string _tableExists = string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';", "Revisions");
        private readonly SQLiteDB _db;

        public RevisionMigrator(SQLiteDB db)
        {
            _db = db;
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
                    _db.ExecuteNonQuery(reader.ReadToEnd());

                    SetMigrated(migration);
                }
            }
        }

        private void SetMigrated(string migration)
        {

            _db.ExecuteNonQuery($"INSERT INTO Revisions (Name, MigratedAt) VALUES ('{migration}',  datetime('now'))");
        }

        private bool HasMigrated(string migration)
        {

            return ((long?)_db.ExecuteScalar($"SELECT ID From Revisions WHERE Name = '{migration}'")).HasValue;
        }

        private IEnumerable<string> Migrations()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(res => res.StartsWith("SortingHat.DB.Migrations"));
        }

        public void Initialize()
        {
            _db.ExecuteNonQuery(CreateRevisionTableCommand);
        }
    }
}
