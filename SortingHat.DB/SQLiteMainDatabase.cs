using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public class SQLiteMainDatabase : SQLiteDatabase, IMainDatabase
    {
        public SQLiteMainDatabase(Func<ISettings> settings, Func<string, IProjectDatabase> projectDatabase, DatabaseSettings databaseSettings) :
            base(databaseSettings, databaseSettings.Name)
        {
            _settings = settings;
            ProjectDatabase = projectDatabase(ProjectDatabaseName);
        }

        public string ProjectDatabaseName => "Default";


        public IProjectDatabase ProjectDatabase { get; }
        public IEnumerable<string> ProjectDatabases => Enumerable.Empty<string>();

        private readonly Func<ISettings> _settings;
        public ISettings Settings => _settings();

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        internal override MigrationType MigrationType => MigrationType.Main;
    }
}
