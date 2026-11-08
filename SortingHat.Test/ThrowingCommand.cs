using System;
using System.Threading;
using System.Threading.Tasks;
using SortingHat.API.DI;

namespace SortingHat.Test;

/// <summary>A command with a bug: it throws an exception hat does not expect.</summary>
internal sealed class ThrowingCommand : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "crash";
    public string Description => "Crashes on purpose.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => throw new InvalidOperationException("broke on purpose");
}
