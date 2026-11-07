using System;
using System.IO;
using System.Linq;
using Funcky.Monads;
using SortingHat.CLI.Commands.Files;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

public class ExportPlanTest
{
    private static readonly string Destination = Path.Combine("dest");
    private static readonly string FirstA = Path.Combine("one", "a.txt");
    private static readonly string SecondA = Path.Combine("two", "a.txt");
    private static readonly string B = Path.Combine("one", "b.txt");

    [Fact]
    public void GivenFreeNamesThenEveryFileGoesIntoTheDestination()
        => Assert.Equal(
            [Target(FirstA, "a.txt"), Target(B, "b.txt")],
            ExportPlan.Targets([FirstA, B], Destination, [], StringComparer.Ordinal));

    [Fact]
    public void GivenANameThatExistsInTheDestinationThenThatFileIsSkipped()
    {
        var plan = ExportPlan.Targets([FirstA, B], Destination, ["a.txt"], StringComparer.Ordinal);

        Assert.True(IsSkipped(plan[0]));
        Assert.Equal(Target(B, "b.txt"), plan[1]);
    }

    [Fact]
    public void GivenTwoFilesWithTheSameNameThenOnlyTheFirstGoesIntoTheDestination()
    {
        var plan = ExportPlan.Targets([FirstA, SecondA], Destination, [], StringComparer.Ordinal);

        Assert.Equal(Target(FirstA, "a.txt"), plan[0]);
        Assert.True(IsSkipped(plan[1]));
    }

    [Fact]
    public void GivenAFileSystemThatIgnoresCaseThenNamesDifferingOnlyInCaseCollide()
    {
        var plan = ExportPlan.Targets([FirstA], Destination, ["A.TXT"], StringComparer.OrdinalIgnoreCase);

        Assert.True(IsSkipped(plan.Single()));
    }

    [Fact]
    public void GivenASkippedFileThenTheReasonNamesTheFileAndItsTarget()
    {
        var reason = ExportPlan.Targets([FirstA], Destination, ["a.txt"], StringComparer.Ordinal)
            .Single()
            .Match(left: Identity, right: _ => string.Empty);

        Assert.Contains(FirstA, reason);
        Assert.Contains(Path.Combine(Destination, "a.txt"), reason);
    }

    private static Either<string, ExportTarget> Target(string source, string name)
        => Either<string, ExportTarget>.Right(new ExportTarget(source, Path.Combine(Destination, name)));

    private static bool IsSkipped(Either<string, ExportTarget> step)
        => step.Match(left: True, right: False);
}
