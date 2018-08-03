using SortingHat.API.DI;
using SortingHat.DB;
using System;

namespace SortingHat.CLI
{
    class CLIService : IServices
    {

        internal CLIService()
        {
            Logger = new CLILogger();
            DB = new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        public ILogger Logger { get; }

        public IDatabase DB { get; }
    }
}
