namespace SortingHat.DB;

/// <summary>
/// A migration in C#, for changes SQL alone cannot express. It runs once, in its own transaction, ordered with the <c>*.migration</c>
/// scripts by its <see cref="Name"/>, which starts with a timestamp like theirs (<c>20261108-1200-TagNameGrammar</c>). Never rename one that ran.
/// </summary>
internal interface ICodeMigration
{
    string Name { get; }

    MigrationType MigrationType { get; }

    void Run(SQLiteDatabase db);
}
