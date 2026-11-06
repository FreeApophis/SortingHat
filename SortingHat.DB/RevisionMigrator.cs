using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using static Funcky.Functional;

namespace SortingHat.DB;

/// <summary>
/// Runs the <c>*.migration</c> scripts embedded in <paramref name="migrations"/> that have not run yet, each in its own transaction.
/// </summary>
internal class RevisionMigrator(ILogger<RevisionMigrator> logger, Assembly migrations) : IRevisionMigrator
{
    private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";

    [UsedImplicitly]
    public RevisionMigrator(ILogger<RevisionMigrator> logger)
        : this(logger, typeof(RevisionMigrator).Assembly)
    {
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

    /// <summary>
    /// Initialized the Migrations Table, this call is idempotent and can be called at any time.
    /// </summary>
    public void Initialize(SQLiteDatabase db)
    {
        db.ExecuteNonQuery(CreateRevisionTableCommand);
    }

    private void RunMigration(SQLiteDatabase db, string migration)
    {
        logger.Log(LogLevel.Information, ($"Run migration '{migration}' on database '{db.DbName}'"));

        using var stream = migrations.GetManifestResourceStream(migration);
        if (stream is { })
        {
            using var reader = new StreamReader(stream);
            var script = reader.ReadToEnd();

            db.InTransaction(() =>
            {
                db.ExecuteNonQuery(script);
                SetMigrated(db, migration);

                return true;
            });
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
        return migrations
            .GetManifestResourceNames()
            .Where(res => res.StartsWith($"{migrations.GetName().Name}.Migrations.{db.MigrationType}"))
            .OrderBy(Identity);
    }
}
