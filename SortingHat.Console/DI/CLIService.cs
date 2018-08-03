using SortingHat.API.DI;
using SortingHat.DB;
using System;
using System.Security.Cryptography;

namespace SortingHat.CLI
{
    class CLIService : IServices
    {

        internal CLIService()
        {
            Logger = new CLILogger();
            DB = new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            HashService = null;
        }

        public ILogger Logger { get; }
        public IDatabase DB { get; }
        public IHashService HashService { get; }
    }
}
