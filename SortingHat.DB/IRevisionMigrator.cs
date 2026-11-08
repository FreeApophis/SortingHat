using System.Collections.Generic;

namespace SortingHat.DB;

internal interface IRevisionMigrator
{
    void Migrate(SQLiteDatabase db);

    /// <summary>The revisions this hat knows for databases of the <paramref name="migrationType"/>, as the Revisions table records them.</summary>
    IReadOnlySet<string> Revisions(MigrationType migrationType);
}