using System.Reflection;
using apophis.CLI;

namespace SortingHat.CLI
{
    class ConsoleApplicationInformationProvider : IConsoleApplicationInformationProvider
    {
        private string FallBackName = "hat";
        public string Name =>
            Assembly.GetEntryAssembly() is {} entryAssembly
                ? entryAssembly.GetName().Name ?? FallBackName
                : FallBackName;
    }
}
