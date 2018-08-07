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
            DB = new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hat");
            HashService = new HashService(SHA256.Create(), nameof(SHA256));
        }

        public ILogger Logger { get; }
        public IDatabase DB { get; }
        public HashService HashService { get; }
    }
}
