using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Plugin;

/// <summary>Finds the plugins in a listing of the plugins folder: every plugin is a folder named like its assembly, plugins/&lt;name&gt;/&lt;name&gt;.dll.</summary>
internal static class PluginLayout
{
    private const string AssemblyExtension = ".dll";

    public static IEnumerable<Either<PluginProblem, PluginCandidate>> Find(IEnumerable<string> looseFiles, IEnumerable<PluginFolder> folders)
        => looseFiles
            .Where(IsAssembly)
            .Select(LooseAssembly)
            .Concat(folders.Select(Candidate));

    private static bool IsAssembly(string file)
        => Path.GetExtension(file).Equals(AssemblyExtension, StringComparison.OrdinalIgnoreCase);

    private static Either<PluginProblem, PluginCandidate> LooseAssembly(string file)
        => Either<PluginProblem, PluginCandidate>.Left(new PluginProblem(
            Path.GetFileName(file),
            $"a plugin must be in its own folder, such as plugins/{Path.GetFileNameWithoutExtension(file)}/{Path.GetFileName(file)}"));

    private static Either<PluginProblem, PluginCandidate> Candidate(PluginFolder folder)
        => folder.Files
            .FirstOrNone(file => Path.GetFileName(file).Equals(AssemblyName(folder), StringComparison.OrdinalIgnoreCase))
            .Match(
                none: () => Either<PluginProblem, PluginCandidate>.Left(new PluginProblem(Name(folder), $"the folder has no {AssemblyName(folder)}")),
                some: path => Either<PluginProblem, PluginCandidate>.Right(new PluginCandidate(Name(folder), path)));

    private static string Name(PluginFolder folder)
        => Path.GetFileName(folder.Path);

    private static string AssemblyName(PluginFolder folder)
        => Name(folder) + AssemblyExtension;
}
