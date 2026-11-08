using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.Events;

namespace SortingHat.Test;

/// <summary>A handler like a plugin would register it, for every file event: it says what it heard.</summary>
internal sealed class FileEventReporter(IConsoleWriter consoleWriter)
    : IFileEventHandler<FileAdded>, IFileEventHandler<FileChanged>, IFileEventHandler<FileTagged>, IFileEventHandler<FileUntagged>, IFileEventHandler<FileMoved>
{
    public Task Handle(FileAdded fileEvent, CancellationToken cancellationToken)
        => Heard($"added {Name(fileEvent.File.Path)}");

    public Task Handle(FileChanged fileEvent, CancellationToken cancellationToken)
        => Heard($"changed {Name(fileEvent.Current.Path)} from {fileEvent.Stored.File.Hash.Short} to {fileEvent.Current.File.Hash.Short}");

    public Task Handle(FileTagged fileEvent, CancellationToken cancellationToken)
        => Heard($"tagged {Name(fileEvent.File.Path)} with {string.Join(", ", fileEvent.Tags.Select(tag => tag.FullName))}");

    public Task Handle(FileUntagged fileEvent, CancellationToken cancellationToken)
        => Heard($"untagged {Name(fileEvent.File.Path)} from {string.Join(", ", fileEvent.Tags.Select(tag => tag.FullName))}");

    public Task Handle(FileMoved fileEvent, CancellationToken cancellationToken)
        => Heard($"moved {Name(fileEvent.File.Path)} to {fileEvent.Destination}");

    private static string Name(string path)
        => Path.GetFileName(path);

    private Task Heard(string what)
    {
        consoleWriter.WriteLine($"heard: {what}");

        return Task.CompletedTask;
    }
}
