using SortingHat.API.DI;
using SortingHat.DB;

namespace SortingHat.CLI
{
    class CLIService : IServices
    {

        internal CLIService()
        {
            Logger = new CLILogger();
            DB = new SQLiteDB();
        }

        public ILogger Logger { get; }

        public IDatabase DB { get; }
    }
}
