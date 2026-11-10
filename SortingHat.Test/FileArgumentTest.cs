using System.IO;
using SortingHat.API;
using Xunit;

namespace SortingHat.Test;

/// <summary>The rules of a file argument, as a pure function: no file system needed.</summary>
public sealed class FileArgumentTest
{
    private static readonly string CurrentDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "work"));

    [Fact]
    public void GivenARelativePathThenItIsResolvedAgainstTheCurrentDirectory()
        => Assert.Equal($"path {Path.Combine(CurrentDirectory, "docs", "a.pdf")}", Parse(Path.Combine("docs", "a.pdf")));

    [Fact]
    public void GivenAFolderWithATrailingSeparatorThenTheSeparatorIsDropped()
        => Assert.Equal($"path {Path.Combine(CurrentDirectory, "docs")}", Parse("docs" + Path.DirectorySeparatorChar));

    [Theory]
    [InlineData("*.pdf")]
    [InlineData("scan-??.jpg")]
    public void GivenWildcardsInTheFileNameThenItIsAPatternInItsFolder(string pattern)
        => Assert.Equal($"pattern {pattern} in {Path.Combine(CurrentDirectory, "docs")}", Parse(Path.Combine("docs", pattern)));

    [Fact]
    public void GivenAWildcardInAFolderThenTheArgumentIsRejected()
        => Assert.Equal($"problem '{Path.Combine("do*", "a.pdf")}': wildcards are only allowed in the file name.", Parse(Path.Combine("do*", "a.pdf")));

    private static string Parse(string argument)
        => FileArgument.Parse(argument, CurrentDirectory).Match(
            left: reason => $"problem {reason}",
            right: parsed => parsed switch
            {
                FileArgument.Exact exact => $"path {exact.Path}",
                FileArgument.Wildcard wildcard => $"pattern {wildcard.Pattern} in {wildcard.Folder}",
            });
}
