using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace SortingHat.DB;

/// <summary>
/// Runs the <c>*.migration</c> scripts embedded in <paramref name="migrations"/> and the <paramref name="codeMigrations"/> that have not run yet,
/// ordered by their names (which start with a timestamp), each in its own transaction.
/// </summary>
internal class RevisionMigrator(ILogger<RevisionMigrator> logger, Assembly migrations, IEnumerable<ICodeMigration> codeMigrations) : IRevisionMigrator
{
    private const string CreateRevisionTableCommand = @"CREATE TABLE IF NOT EXISTS [Revisions] ([Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(255)  UNIQUE NOT NULL, [MigratedAt] DATETIME DEFAULT CURRENT_TIME NOT NULL);";

    [UsedImplicitly]
    public RevisionMigrator(ILogger<RevisionMigrator> logger, IEnumerable<ICodeMigration> codeMigrations)
        : this(logger, typeof(RevisionMigrator).Assembly, codeMigrations)
    {
    }

    public void Migrate(SQLiteDatabase db)
    {
        Initialize(db);

        foreach (var migration in Migrations(db.MigrationType).Where(migration => !HasMigrated(db, migration.Revision)))
        {
            Run(db, migration);
        }
    }

    public IReadOnlySet<string> Revisions(MigrationType migrationType)
        => Migrations(migrationType).Select(migration => migration.Revision).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Initialized the Migrations Table, this call is idempotent and can be called at any time.
    /// </summary>
    public void Initialize(SQLiteDatabase db)
    {
        db.ExecuteNonQuery(CreateRevisionTableCommand);
    }

    private static void SetMigrated(SQLiteDatabase db, string revision)
        => db.ExecuteNonQuery("INSERT INTO Revisions (Name, MigratedAt) VALUES (@migration,  datetime('now'))", new SqliteParameter("@migration", revision));

    private static bool HasMigrated(SQLiteDatabase db, string revision)
        => db.ExecuteScalar("SELECT Id From Revisions WHERE Name = @migration", new SqliteParameter("@migration", revision)) is long;

    private void Run(SQLiteDatabase db, Migration migration)
    {
        logger.Log(LogLevel.Information, $"Run migration '{migration.Revision}' on database '{db.DbName}'");

        db.InTransaction(() =>
        {
            migration.Apply(db);
            SetMigrated(db, migration.Revision);

            return true;
        });
    }

    /// <summary>A script is recorded by its resource name, a code migration by its name: never change either once it ran.</summary>
    private IEnumerable<Migration> Migrations(MigrationType migrationType)
    {
        var prefix = $"{migrations.GetName().Name}.Migrations.{migrationType}.";

        var scripts = migrations
            .GetManifestResourceNames()
            .Where(resource => resource.StartsWith(prefix, StringComparison.Ordinal))
            .Select(resource => new Migration(resource[prefix.Length..], resource, database => database.ExecuteNonQuery(Script(resource))));

        var code = codeMigrations
            .Where(migration => migration.MigrationType == migrationType)
            .Select(migration => new Migration(migration.Name, migration.Name, migration.Run));

        return scripts.Concat(code).OrderBy(migration => migration.Order, StringComparer.Ordinal);
    }

    private string Script(string resource)
    {
        using var stream = migrations.GetManifestResourceStream(resource) ?? throw new InvalidOperationException($"The migration '{resource}' is missing.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    /// <param name="Order">The name without its folder, starting with the timestamp.</param>
    /// <param name="Revision">The name recorded in the Revisions table.</param>
    /// <param name="Apply">Changes the database, inside the transaction of the migration.</param>
    private sealed record Migration(string Order, string Revision, Action<SQLiteDatabase> Apply);
}
