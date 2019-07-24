using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public class SQLiteMainDatabase : IMainDatabase, ISQLiteDatabase, IDisposable
    {
        public SQLiteMainDatabase(IProjectDatabase projectDatabase)
        {
            ProjectDatabase = projectDatabase;
        }

        public IProjectDatabase ProjectDatabase { get; }
        public IReadOnlyCollection<string> ProjectDatabases { get; }
        public ISettings Settings { get; }

        public void Setup()
        {
            throw new System.NotImplementedException();
        }

        public void TearDown()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        public void ExecuteNonQuery(string commandText, params SqliteParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string commandText, params SqliteParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public SqliteDataReader ExecuteReader(string commandText, params SqliteParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
