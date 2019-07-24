using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SortingHat.DB
{
    internal class RevisionMigrator
    {
        private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";
        private readonly SQLiteDatabase _db;

        public RevisionMigrator(SQLiteDatabase db)
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

                RunMigration(migration);
            }
        }

        private void RunMigration(string migration)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(migration);
            using var reader = new StreamReader(stream);

            _db.ExecuteNonQuery(reader.ReadToEnd());

            SetMigrated(migration);
        }

        private void SetMigrated(string migration)
        {
            _db.ExecuteNonQuery("INSERT INTO Revisions (Name, MigratedAt) VALUES (@migration,  datetime('now'))", new SqliteParameter("@migration", migration));
        }

        private bool HasMigrated(string migration)
        {

            return ((long?)_db.ExecuteScalar("SELECT ID From Revisions WHERE Name = @migration", new SqliteParameter("@migration", migration))).HasValue;
        }

        private IEnumerable<string> Migrations()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(res => res.StartsWith($"SortingHat.DB.Migrations.{_db.MigrationType}"));
        }

        /// <summary>
        /// Initialized the Migrations Table, this call is idempotent and can be called at any time.
        /// </summary>
        public void Initialize()
        {
            _db.ExecuteNonQuery(CreateRevisionTableCommand);
        }
    }
}
