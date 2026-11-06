using System.Reflection;
using Apophis.CLI;

namespace SortingHat.CLI;

internal class ConsoleApplicationInformationProvider : IConsoleApplicationInformationProvider
{
    private string _FallBackName = "hat";

    public string Name =>
        Assembly.GetEntryAssembly() is { } entryAssembly
            ? entryAssembly.GetName().Name ?? _FallBackName
            : _FallBackName;
}
