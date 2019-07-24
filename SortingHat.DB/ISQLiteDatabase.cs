using Microsoft.Data.Sqlite;

namespace SortingHat.DB
{
    interface ISQLiteDatabase
    {
        void ExecuteNonQuery(string commandText, params SqliteParameter[] parameters);

        object ExecuteScalar(string commandText, params SqliteParameter[] parameters);

        SqliteDataReader ExecuteReader(string commandText, params SqliteParameter[] parameters);
    }
}
