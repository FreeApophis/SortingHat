using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SortingHat.API.DI;

/// <summary>
/// A command that declares its arguments and options. The host parses and checks the command line against them,
/// reports mistakes before the command runs, and generates the help for <c>hat &lt;command&gt; --help</c>.
/// Register implementations as <see cref="IDeclaredCommand"/> in a module; this is the successor of <see cref="ICommand"/>.
/// </summary>
public interface IDeclaredCommand
{
    CommandGrouping CommandGrouping { get; }

    /// <summary>The name on the command line, for example <c>find-files</c>.</summary>
    string Name { get; }

    /// <summary>Other names for the command, for example <c>ff</c>.</summary>
    IReadOnlyList<string> Aliases => [];

    /// <summary>A single line description.</summary>
    string Description { get; }

    /// <summary>The positional arguments, in their order on the command line. Only the last one should take more than one value.</summary>
    IReadOnlyList<ArgumentDefinition> Arguments => [];

    /// <summary>The options this command accepts, anywhere on the command line.</summary>
    IReadOnlyList<OptionDefinition> Options => [];

    /// <summary>Runs the command with the parsed command line. Returns whether it succeeded, which decides the exit code.</summary>
    Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken);
}
