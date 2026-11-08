using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.Events;

[UsedImplicitly]
internal sealed class FileEvents<TEvent>(IEnumerable<IFileEventHandler<TEvent>> handlers) : IFileEvents<TEvent>
{
    public async Task<IReadOnlyList<string>> Publish(TEvent fileEvent, CancellationToken cancellationToken)
    {
        var problems = new List<string>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.Handle(fileEvent, cancellationToken);
            }

            // a handler comes from a plugin: whatever it throws must not undo a change that is stored already
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                problems.Add($"Warning: {handler.GetType().Name} ({Source(handler)}) failed: {exception.Message}");
            }
        }

        return problems;
    }

    private static string Source(IFileEventHandler<TEvent> handler)
        => Option.FromNullable(handler.GetType().Assembly.GetName().Name).GetOrElse("unknown assembly");
}
