using System;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access;

internal class SQLiteSettings(SQLiteMainDatabase db) : ISettings
{
    public string? this[string key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public bool HasValue(string key)
    {
        var result = db.ExecuteScalar("SELECT Id FROM [SETTINGS] WHERE Key = @key", new SqliteParameter("@key", key));

        return result != null;
    }

    private string? GetValue(string key)
    {
        // null if the key does not exist, DBNull if the value is NULL
        return db.ExecuteScalar("SELECT [Value] FROM [SETTINGS] WHERE Key = @key", new SqliteParameter("@key", key)) as string;
    }

    private void SetValue(string key, string? value)
    {
        db.ExecuteNonQuery(
            "INSERT INTO [Settings] ([Key], [Value]) VALUES (@key, @value)",
            new SqliteParameter("@key", key),
            new SqliteParameter("@value", (object?)value ?? DBNull.Value));
    }
}
