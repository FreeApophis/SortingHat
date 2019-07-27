using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access
{
    class SQLiteSettings : ISettings
    {
        private readonly SQLiteMainDatabase _db;

        public SQLiteSettings(SQLiteMainDatabase db)
        {
            _db = db;
        }

        public string this[string key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public bool HasValue(string key)
        {
            var result = _db.ExecuteScalar("SELECT Id FROM [SETTINGS] WHERE Key = @key", new SqliteParameter("@key", key));

            return result != null;
        }

        private string GetValue(string key)
        {
            var result = _db.ExecuteScalar("SELECT [Value] FROM [SETTINGS] WHERE Key = @key", new SqliteParameter("@key", key));

            return (string)result;
        }

        private void SetValue(string key, string value)
        {
            _db.ExecuteNonQuery("INSERT INTO [Settings] ([Key], [Value]) VALUES (@key, @value)",
                new SqliteParameter("@key", key),
                new SqliteParameter("@value", value));
        }
    }
}
