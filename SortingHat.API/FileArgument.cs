using System.IO;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.API;

/// <summary>A file argument on the command line: the path of a file or folder, or a pattern with wildcards in its file name such as 'C:\scans\*.pdf'.</summary>
internal closed record FileArgument
{
    private FileArgument(string argument)
        => Argument = argument;

    /// <summary>The argument as the user typed it, for messages.</summary>
    public string Argument { get; }

    /// <summary>Resolves a relative <paramref name="argument"/> against <paramref name="currentDirectory"/>; wildcards in a folder name are not allowed.</summary>
    public static Either<string, FileArgument> Parse(string argument, string currentDirectory)
        => Classify(argument, Path.TrimEndingDirectorySeparator(Path.GetFullPath(argument, currentDirectory)));

    private static Either<string, FileArgument> Classify(string argument, string fullPath)
        => Option.FromNullable(Path.GetDirectoryName(fullPath)).Match(
            none: () => Either<string, FileArgument>.Right(new Exact(argument, fullPath)),
            some: folder => HasWildcard(folder)
                ? Either<string, FileArgument>.Left($"'{argument}': wildcards are only allowed in the file name.")
                : HasWildcard(Path.GetFileName(fullPath))
                    ? Either<string, FileArgument>.Right(new Wildcard(argument, folder, Path.GetFileName(fullPath)))
                    : Either<string, FileArgument>.Right(new Exact(argument, fullPath)));

    private static bool HasWildcard(string path)
        => path.Any(character => character is '*' or '?');

    /// <summary>A path without wildcards, a file or a folder.</summary>
    public sealed record Exact : FileArgument
    {
        internal Exact(string argument, string path)
            : base(argument)
            => Path = path;

        /// <summary>The full path.</summary>
        public string Path { get; }
    }

    /// <summary>A <see cref="Pattern"/> such as '*.pdf' for the files in <see cref="Folder"/>.</summary>
    public sealed record Wildcard : FileArgument
    {
        internal Wildcard(string argument, string folder, string pattern)
            : base(argument)
            => (Folder, Pattern) = (folder, pattern);

        /// <summary>The full path of the folder.</summary>
        public string Folder { get; }

        public string Pattern { get; }
    }
}
