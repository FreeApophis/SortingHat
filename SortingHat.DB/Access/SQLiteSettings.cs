using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access;

internal class SQLiteSettings(SQLiteMainDatabase db) : ISettings
{
    /// <summary>A key without a row and a key whose value is NULL both have no value.</summary>
    public Option<string> Get(string key)
        => Option.FromNullable(db.ExecuteScalar("SELECT [Value] FROM [Settings] WHERE [Key] = @key", new SqliteParameter("@key", key)) as string);

    /// <summary>The key is unique with ON CONFLICT REPLACE, so setting it again replaces its row.</summary>
    public void Set(string key, string value)
        => db.ExecuteNonQuery(
            "INSERT INTO [Settings] ([Key], [Value]) VALUES (@key, @value)",
            new SqliteParameter("@key", key),
            new SqliteParameter("@value", value));
}
