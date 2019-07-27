using System;
using System.Collections.Generic;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public class SQLiteMainDatabase : SQLiteDatabase, IMainDatabase
    {
        public SQLiteMainDatabase(DatabaseSettings databaseSettings) :
            base(databaseSettings, databaseSettings.Name)
        {
        }

        public Dictionary<string, long> GetStatistics()
        {
            throw new System.NotImplementedException();
        }

        internal override MigrationType MigrationType => MigrationType.Main;
    }
}
