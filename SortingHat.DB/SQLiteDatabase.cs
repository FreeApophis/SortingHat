using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API;

namespace SortingHat.DB;

/// <summary>
/// A SQLite database that opens on its first use: only then is its name decided, its file created when it is missing,
/// and its migrations run. A command that never touches the database leaves no file behind.
/// </summary>
public abstract class SQLiteDatabase : IDisposable
{
    private readonly string _path;
    private readonly Lazy<string> _dbName;
    private readonly IRevisionMigrator _migrator;

    private bool _disposedValue;
    private Option<SqliteConnection> _connection;
    private Option<SqliteTransaction> _transaction;

    /// <param name="databaseSettings">Where the database files are.</param>
    /// <param name="dbName">The name of the database file, asked for on the first use.</param>
    /// <param name="migrator">Brings the database up to date when it opens.</param>
    private protected SQLiteDatabase(DatabaseSettings databaseSettings, Func<string> dbName, IRevisionMigrator migrator)
    {
        if (databaseSettings.Type != "sqlite")
        {
            throw new NotSupportedException("Only SQLite is currently supported as Database Type");
        }

        _path = databaseSettings.DbPath;
        _dbName = new Lazy<string>(dbName);
        _migrator = migrator;
    }

    public string DbName
        => _dbName.Value;

    internal abstract MigrationType MigrationType { get; }

    private SqliteConnection Connection
        => _connection.GetOrElse(Open);

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

    /// <summary>The number of rows in each of the <paramref name="tables"/>. The table names come from the code, never from input.</summary>
    private protected IReadOnlyDictionary<string, long> CountRows(IReadOnlyList<string> tables)
        => Query(
                string.Join(" UNION ALL ", tables.Select(table => $"SELECT '{table}', COUNT(*) FROM [{table}]")),
                reader => KeyValuePair.Create(reader.GetString(0), reader.GetInt64(1)))
            .ToDictionary();

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

    /// <summary>
    /// The connection is known before the migrations run, because they use it. When opening or a migration fails,
    /// the connection is closed again, so the file is not kept open, and the next use tries again.
    /// </summary>
    private SqliteConnection Open()
    {
        // without the pool, disposing the database closes its file, so a removed project's file can be deleted
        var connection = new SqliteConnection($"Filename={DbFile()};Pooling=False");
        _connection = connection;

        try
        {
            connection.Open();
            ExecuteNonQuery("PRAGMA foreign_keys=ON;");
            _migrator.Migrate(this);

            return connection;
        }
        catch
        {
            _connection = Option<SqliteConnection>.None;
            connection.Dispose();

            throw;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _connection.AndThen(connection => connection.Dispose());
            }

            _disposedValue = true;
        }
    }
}
