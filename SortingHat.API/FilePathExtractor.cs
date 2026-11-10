using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.API;

/// <summary>Finds the files of the file arguments in the file system.</summary>
[UsedImplicitly]
internal class FilePathExtractor : IFilePathExtractor
{
    public Either<IReadOnlyList<string>, IReadOnlyList<string>> FromFilePatterns(IEnumerable<string> filePatterns, bool recursive)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var found = filePatterns
            .Select(pattern => FileArgument.Parse(pattern, currentDirectory).SelectMany(argument => Files(argument, recursive)))
            .ToList();
        var problems = found.WhereSelect(files => files.LeftOrNone()).ToList();

        return problems is []
            ? Either<IReadOnlyList<string>, IReadOnlyList<string>>.Right([.. found.WhereSelect(files => files.RightOrNone()).Flatten().Distinct()])
            : Either<IReadOnlyList<string>, IReadOnlyList<string>>.Left(problems);
    }

    private static Either<string, IReadOnlyList<string>> Files(FileArgument argument, bool recursive)
        => argument switch
        {
            FileArgument.Exact exact => File.Exists(exact.Path)
                ? Either<string, IReadOnlyList<string>>.Right([RealCase(exact.Path)])
                : Directory.Exists(exact.Path)
                    ? Folder(exact, recursive)
                    : Either<string, IReadOnlyList<string>>.Left($"'{argument.Argument}' is no file or folder."),
            FileArgument.Wildcard wildcard => Directory.Exists(wildcard.Folder)
                ? Search(argument, RealCase(wildcard.Folder), wildcard.Pattern, recursive)
                : Either<string, IReadOnlyList<string>>.Left($"'{argument.Argument}' matches no file, there is no folder '{wildcard.Folder}'."),
        };

    /// <summary>A folder means everything in it, so it needs recursion, like 'cp -r': naming a folder by mistake does not take all its files.</summary>
    private static Either<string, IReadOnlyList<string>> Folder(FileArgument.Exact folder, bool recursive)
        => recursive
            ? Search(folder, RealCase(folder.Path), "*", recursive)
            : Either<string, IReadOnlyList<string>>.Left($"'{folder.Argument}' is a folder, use -r for everything in it or '{Path.Combine(folder.Argument, "*")}' for its files.");

    private static Either<string, IReadOnlyList<string>> Search(FileArgument argument, string folder, string pattern, bool recursive)
    {
        try
        {
            IReadOnlyList<string> files = [.. Enumerate(folder, pattern, recursive).Order(StringComparer.Ordinal)];

            return files is []
                ? Either<string, IReadOnlyList<string>>.Left($"'{argument.Argument}' matches no file.")
                : Either<string, IReadOnlyList<string>>.Right(files);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Either<string, IReadOnlyList<string>>.Left($"'{argument.Argument}': {exception.Message}");
        }
    }

    /// <summary>The paths keep the real case of the file system, because <paramref name="folder"/> has it.</summary>
    private static IEnumerable<string> Enumerate(string folder, string pattern, bool recursive)
        => new FileSystemEnumerable<string>(folder, (ref entry) => entry.ToFullPath(), Options(recursive))
        {
            ShouldIncludePredicate = (ref entry) => !entry.IsDirectory && Matches(pattern, entry.FileName),
            ShouldRecursePredicate = (ref entry) => (entry.Attributes & FileAttributes.ReparsePoint) == 0,
        };

    /// <summary>Includes hidden and system files like the file system calls hat used before; links are found, <see cref="Enumerate"/> does not follow them.</summary>
    private static EnumerationOptions Options(bool recursive)
        => new() { IgnoreInaccessible = true, AttributesToSkip = 0, RecurseSubdirectories = recursive };

    /// <summary>Like the shell of the platform: on Windows '*.*' matches every file, also one without an extension.</summary>
    private static bool Matches(string pattern, ReadOnlySpan<char> name)
        => OperatingSystem.IsWindows()
            ? FileSystemName.MatchesWin32Expression(FileSystemName.TranslateWin32Expression(pattern), name)
            : FileSystemName.MatchesSimpleExpression(pattern, name, ignoreCase: false);

    /// <summary>
    /// The <paramref name="fullPath"/> with the case of the file system, so 'c:\Scans\a.pdf' and 'C:\scans\a.pdf' are one path in the database.
    /// A name hat cannot read keeps the case it was typed in.
    /// </summary>
    private static string RealCase(string fullPath)
        => Option.FromNullable(Path.GetPathRoot(fullPath)).Match(
            none: fullPath,
            some: root => fullPath[root.Length..]
                .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(DriveCase(root), (folder, name) => Path.Combine(folder, RealName(folder, name))));

    private static string DriveCase(string root)
        => OperatingSystem.IsWindows() && root is [_, ':', ..] ? root.ToUpperInvariant() : root;

    private static string RealName(string folder, string name)
    {
        try
        {
            return new DirectoryInfo(folder).EnumerateFileSystemInfos(name, Options(recursive: false)).Select(entry => entry.Name).FirstOrNone().GetOrElse(name);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return name;
        }
    }
}
