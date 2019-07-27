using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace SortingHat.DB
{
    internal class RevisionMigrator : IRevisionMigrator
    {
        private readonly ILogger<RevisionMigrator> _logger;
        private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";

        public RevisionMigrator(ILogger<RevisionMigrator> logger)
        {
            _logger = logger;
        }

        public void Migrate(SQLiteDatabase db)
        {
            Initialize(db);

            foreach (var migration in Migrations(db))
            {
                if (HasMigrated(db, migration))
                {
                    continue;
                }

                RunMigration(db, migration);
            }
        }

        private void RunMigration(SQLiteDatabase db, string migration)
        {
            _logger.Log(LogLevel.Information, ($"Run migration '{migration}' on database '{db.DbName}'"));

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(migration);
            if (stream is { })
            {
                using var reader = new StreamReader(stream);

                db.ExecuteNonQuery(reader.ReadToEnd());

                SetMigrated(db, migration);
            }
        }

        private void SetMigrated(SQLiteDatabase db, string migration)
        {
            db.ExecuteNonQuery("INSERT INTO Revisions (Name, MigratedAt) VALUES (@migration,  datetime('now'))", new SqliteParameter("@migration", migration));
        }

        private bool HasMigrated(SQLiteDatabase db, string migration)
        {

            return ((long?)db.ExecuteScalar("SELECT Id From Revisions WHERE Name = @migration", new SqliteParameter("@migration", migration))).HasValue;
        }

        private IEnumerable<string> Migrations(SQLiteDatabase db)
        {
            return Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(res => res.StartsWith($"SortingHat.DB.Migrations.{db.MigrationType}"))
                .OrderBy(n => n);
        }

        /// <summary>
        /// Initialized the Migrations Table, this call is idempotent and can be called at any time.
        /// </summary>
        public void Initialize(SQLiteDatabase db)
        {
            db.ExecuteNonQuery(CreateRevisionTableCommand);
        }
    }
}
