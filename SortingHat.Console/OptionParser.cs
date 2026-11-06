using System.Collections.Generic;
using System.Linq;
using SortingHat.API.DI;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI;

/// <summary>The options given on the command line, for commands that use the <see cref="IOptionParser"/> contract.</summary>
internal sealed class OptionParser(IReadOnlyList<IOption> givenOptions) : IOptionParser
{
    public bool HasOption<TOption>()
        where TOption : IOption, new()
        => givenOptions.Any(option => option is TOption);
}
