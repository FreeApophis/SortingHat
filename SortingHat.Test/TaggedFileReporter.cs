using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.Events;

namespace SortingHat.Test;

/// <summary>A handler like a plugin would register it: it says which file got which tags.</summary>
internal sealed class TaggedFileReporter(IConsoleWriter consoleWriter) : IFileEventHandler<FileTagged>
{
    public Task Handle(FileTagged fileEvent, CancellationToken cancellationToken)
    {
        consoleWriter.WriteLine($"heard: {Path.GetFileName(fileEvent.File.Path)} tagged with {string.Join(", ", fileEvent.Tags.Select(tag => tag.FullName))}");

        return Task.CompletedTask;
    }
}
