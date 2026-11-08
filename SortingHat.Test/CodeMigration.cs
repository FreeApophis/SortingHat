using System;
using SortingHat.DB;

namespace SortingHat.Test;

/// <summary>A migration in C# that runs <paramref name="run"/>, for tests of migrations.</summary>
internal sealed class CodeMigration(string name, MigrationType migrationType, Action<SQLiteDatabase> run) : ICodeMigration
{
    public string Name => name;

    public MigrationType MigrationType => migrationType;

    public void Run(SQLiteDatabase db)
        => run(db);
}
