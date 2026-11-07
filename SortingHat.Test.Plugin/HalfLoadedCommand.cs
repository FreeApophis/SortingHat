using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.Test.Plugin;

/// <summary>Registered by <see cref="FailingModule"/> before it fails; hat must not offer it.</summary>
[UsedImplicitly]
internal sealed class HalfLoadedCommand : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public string Name => "half-loaded";

    public IReadOnlyList<string> Aliases => [];

    public string Description => "Must never be available.";

    public string Details => string.Empty;

    public IReadOnlyList<ArgumentDefinition> Arguments => [];

    public IReadOnlyList<OptionDefinition> Options => [];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(true);
}
