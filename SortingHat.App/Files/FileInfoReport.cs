using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.App.Files;

/// <summary>What file-info tells about one path. hat knows a file by its content, so a path it does not know can still have known content.</summary>
public abstract record FileInfoReport
{
    private FileInfoReport()
    {
    }

    /// <summary>The lines file-info prints for the path.</summary>
    public abstract IReadOnlyList<string> Lines { get; }

    /// <summary>Decides the report from what the database knows about the path and about the content now at the path.</summary>
    /// <param name="stored">What is stored for the path, when the path is known.</param>
    /// <param name="changed">Whether the content at the path differs from the stored one.</param>
    /// <param name="current">What is stored for the content now at the path; it has no paths when the content is unknown.</param>
    public static FileInfoReport Decide(Option<FileDetails> stored, bool changed, FileDetails current)
        => stored.Match<FileInfoReport>(
            none: () => current.Paths.Count == 0 ? new NotIndexed() : new IndexedElsewhere(current),
            some: details => changed ? new Changed(details) : new Indexed(details));

    /// <summary>Neither the path nor its content is known.</summary>
    public sealed record NotIndexed : FileInfoReport
    {
        public override IReadOnlyList<string> Lines => ["File not in index!"];
    }

    /// <summary>The path is known and its content has not changed.</summary>
    public sealed record Indexed(FileDetails Details) : FileInfoReport
    {
        public override IReadOnlyList<string> Lines => Details.Lines;
    }

    /// <summary>The path is known, but the file was edited since; <see cref="Before"/> is what is stored for the old content.</summary>
    public sealed record Changed(FileDetails Before) : FileInfoReport
    {
        public override IReadOnlyList<string> Lines
            => ["The file has changed since it was indexed, 'repair --rehash' carries its tags over to the new content. Before the change:", .. Before.Lines];
    }

    /// <summary>The path is unknown, but the same content is known under other paths, such as a copy.</summary>
    public sealed record IndexedElsewhere(FileDetails Details) : FileInfoReport
    {
        public override IReadOnlyList<string> Lines
            => ["The file is not indexed at this path, but its content is:", .. Details.Lines];
    }
}
