using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.CLI;

/// <summary>A command as hat offers it: under its name and the aliases no command before it claimed.</summary>
internal sealed record OfferedCommand(IDeclaredCommand Command, IReadOnlyList<string> Aliases);
