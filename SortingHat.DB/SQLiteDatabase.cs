using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SortingHat.API;

namespace SortingHat.DB;

public abstract class SQLiteDatabase : IDisposable
{
    private readonly string _path;
    private readonly string _encryptionKey = "Encrypted";

    private bool _disposedValue;

    protected SQLiteDatabase(DatabaseSettings databaseSettings, string dbName)
    {
        if (databaseSettings.Type == "sqlite")
        {
            _path = databaseSettings.DbPath;
            DbName = dbName;

            Connection = new SqliteConnection($"Filename={DbFile()}");
            Connection.Open();

            ExecuteNonQuery("PRAGMA foreign_keys=ON;");

            if (_encryptionKey != null)
            {
                ExecuteNonQuery("PRAGMA key='{_encryptionKey}';");
            }
        }
        else
        {
            throw new NotSupportedException("Only SQLite is currently supported as Database Type");
        }
    }

    public string DbName { get; }

    internal abstract MigrationType MigrationType { get; }

    private SqliteConnection Connection { get; }

    public void Dispose()
    {
        Dispose(true);
    }

    internal void ExecuteNonQuery(string commandText, params SqliteParameter[] parameters)
    {
        using var command = CreateCommand(commandText, parameters);
        command.ExecuteNonQuery();
    }

    internal object? ExecuteScalar(string commandText, params SqliteParameter[] parameters)
    {
        using var command = CreateCommand(commandText, parameters);
        return command.ExecuteScalar();
    }

    internal SqliteDataReader ExecuteReader(string commandText, params SqliteParameter[] parameters)
    {
        return CreateCommand(commandText, parameters).ExecuteReader();
    }

    private string ExistingDbPath()
    {
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        return _path;
    }

    private string DbFile()
    {
        return Path.Combine(ExistingDbPath(), $"{DbName}.db");
    }

    private SqliteCommand CreateCommand(string commandText, params SqliteParameter[] parameters)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = commandText;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        return command;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Connection.Close();
                Connection.Dispose();
            }

            _disposedValue = true;
        }
    }
}
