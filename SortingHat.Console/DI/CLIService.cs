using SortingHat.API.Interfaces;

namespace SortingHat.CLI
{
     class CLIService : IServices
    {

        internal CLIService()
        {
            Logger = new CLILogger();
        }

        public ILogger Logger { get; }
    }
}
