using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.CLI.Commands.Files;

/// <summary>
/// Decides where copy-files and move-files put each file. Nothing is ever overwritten: a file whose name already exists
/// in the destination, or is already taken by an earlier file of the same run, is skipped with the reason.
/// </summary>
internal static class ExportPlan
{
    /// <summary>One step per source, in the same order: its target, or why it is skipped.</summary>
    /// <param name="sourcePaths">The files to put into the destination.</param>
    /// <param name="destinationPath">The destination folder.</param>
    /// <param name="namesInDestination">The file names that already exist in the destination.</param>
    /// <param name="nameComparer">How the file system compares names, for example ignoring case on Windows.</param>
    public static IReadOnlyList<Either<string, ExportTarget>> Targets(IEnumerable<string> sourcePaths, string destinationPath, IEnumerable<string> namesInDestination, StringComparer nameComparer)
    {
        var existing = namesInDestination.ToImmutableHashSet(nameComparer);

        return sourcePaths
            .Aggregate(
                (Steps: ImmutableList<Either<string, ExportTarget>>.Empty, Taken: existing),
                (plan, source) => Plan(plan, source, destinationPath, existing))
            .Steps;
    }

    private static (ImmutableList<Either<string, ExportTarget>> Steps, ImmutableHashSet<string> Taken) Plan(
        (ImmutableList<Either<string, ExportTarget>> Steps, ImmutableHashSet<string> Taken) plan,
        string source,
        string destinationPath,
        ImmutableHashSet<string> existing)
    {
        var name = Path.GetFileName(source);
        var target = Path.Combine(destinationPath, name);

        return plan.Taken.Contains(name)
            ? (plan.Steps.Add(Either<string, ExportTarget>.Left(SkipReason(source, target, existing.Contains(name)))), plan.Taken)
            : (plan.Steps.Add(Either<string, ExportTarget>.Right(new ExportTarget(source, target))), plan.Taken.Add(name));
    }

    private static string SkipReason(string source, string target, bool existsAlready)
        => existsAlready
            ? $"'{source}' skipped: '{target}' already exists."
            : $"'{source}' skipped: another file with the same name already goes to '{target}'.";
}
