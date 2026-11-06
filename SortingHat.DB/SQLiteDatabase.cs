using System;
using System.Collections.Generic;
using System.IO;
using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API;

namespace SortingHat.DB;

public abstract class SQLiteDatabase : IDisposable
{
    private readonly string _path;
    private readonly string _encryptionKey = "Encrypted";

    private bool _disposedValue;
    private Option<SqliteTransaction> _transaction;

    protected SQLiteDatabase(DatabaseSettings databaseSettings, string dbName)
    {
        if (databaseSettings.Type == "sqlite")
        {
            _path = databaseSettings.DbPath;
            DbName = dbName;

            // without the pool, disposing the database closes its file, so a removed project's file can be deleted
            Connection = new SqliteConnection($"Filename={DbFile()};Pooling=False");
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

    /// <summary>
    /// Runs <paramref name="work"/> in a transaction, which is committed when the work returns and rolled back when it throws.
    /// Inside a running transaction the work joins it, so the outermost transaction decides.
    /// </summary>
    internal TResult InTransaction<TResult>(Func<TResult> work)
        => _transaction.Match(none: () => InNewTransaction(work), some: _ => work());

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

    /// <summary>
    /// Lends the reader of the query to <paramref name="read"/>. The reader and its command are disposed afterwards,
    /// so <paramref name="read"/> must not return anything that still reads lazily from the reader.
    /// </summary>
    internal TResult ExecuteReader<TResult>(string commandText, Func<SqliteDataReader, TResult> read, params SqliteParameter[] parameters)
    {
        using var command = CreateCommand(commandText, parameters);
        using var reader = command.ExecuteReader();
        return read(reader);
    }

    /// <summary>
    /// Maps every row of the query with <paramref name="readRow"/>.
    /// </summary>
    internal IReadOnlyList<TRow> Query<TRow>(string commandText, Func<SqliteDataReader, TRow> readRow, params SqliteParameter[] parameters)
        => ExecuteReader(commandText, reader => ReadRows(reader, readRow), parameters);

    private static List<TRow> ReadRows<TRow>(SqliteDataReader reader, Func<SqliteDataReader, TRow> readRow)
    {
        var rows = new List<TRow>();

        while (reader.Read())
        {
            rows.Add(readRow(reader));
        }

        return rows;
    }

    private TResult InNewTransaction<TResult>(Func<TResult> work)
    {
        using var transaction = Connection.BeginTransaction();
        _transaction = transaction;

        try
        {
            var result = work();
            transaction.Commit();

            return result;
        }
        finally
        {
            _transaction = Option<SqliteTransaction>.None;
        }
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
        _transaction.AndThen(transaction => command.Transaction = transaction);

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
