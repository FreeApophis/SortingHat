using System.Collections.Generic;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.DI;

/// <summary>
/// The command line of an <see cref="IDeclaredCommand"/>, parsed and checked against its declaration.
/// </summary>
public sealed class CommandInput(IReadOnlyDictionary<ArgumentDefinition, IReadOnlyList<string>> arguments, IReadOnlySet<OptionDefinition> givenOptions)
{
    /// <summary>All values of the <paramref name="argument"/>, empty when it was not given.</summary>
    public IReadOnlyList<string> Values(ArgumentDefinition argument)
        => arguments.GetValueOrNone(argument).GetOrElse([]);

    /// <summary>The value of an <paramref name="argument"/> that takes one value.</summary>
    public Option<string> Value(ArgumentDefinition argument)
        => Values(argument).FirstOrNone();

    /// <summary>Whether the <paramref name="option"/> was given.</summary>
    public bool Has(OptionDefinition option)
        => givenOptions.Contains(option);
}
