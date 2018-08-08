using Microsoft.Extensions.Configuration;
using SortingHat.API.DI;
using SortingHat.DB;
using System;
using System.Security.Cryptography;

namespace SortingHat.CLI
{
    class CLIService : IServices
    {
        private ConfigurationBuilder configurationBuilder;

        public CLIService(ConfigurationBuilder configurationBuilder)
        {
            this.configurationBuilder = configurationBuilder;
        }

        internal CLIService()
        {
            Logger = new CLILogger();
            DB = new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hat");
            HashService = new HashService(SHA256.Create(), nameof(SHA256));
            Configuration = LoadConfiguration();
        }

        private IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath(System.Reflection.Assembly.GetEntryAssembly().Location);
            configurationBuilder.AddJsonFile("hat.config");

            return configurationBuilder.Build();
        }

        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }
        public IDatabase DB { get; }
        public HashService HashService { get; }
    }
}
