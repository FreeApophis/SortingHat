using System;
using System.Threading;
using System.Threading.Tasks;
using SortingHat.API.Events;

namespace SortingHat.Test;

/// <summary>A handler with a bug, like a broken plugin.</summary>
internal sealed class FailingTaggedFileHandler : IFileEventHandler<FileTagged>
{
    public async Task Handle(FileTagged fileEvent, CancellationToken cancellationToken)
    {
        await Task.Yield();

        throw new InvalidOperationException("the handler broke");
    }
}
