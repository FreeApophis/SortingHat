using System;
using Funcky.Monads;
using SortingHat.API;
using SortingHat.API.AutoTag;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>The values of the built-in variables, as pure rules: folders from a path, parts of a date.</summary>
public sealed class AutoTagValueTest
{
    private static readonly DateTime Taken = new(2019, 3, 5, 7, 8, 9, DateTimeKind.Local);

    [Theory]
    [InlineData(@"C:\User\Path", new[] { "C", "User", "Path" })]
    [InlineData(@"C:\", new[] { "C" })]
    [InlineData("/home/user", new[] { "home", "user" })]
    [InlineData(@"\\server\share\docs", new[] { "server", "share", "docs" })]
    [InlineData(@"C:\User\Path\", new[] { "C", "User", "Path" })]
    public void GivenAFolderThenItsFoldersFromTheRootAreItsPathWithTheDriveAsItsLetter(string folder, string[] folders)
        => Assert.Equal(folders, PathHelper.Folders(folder));

    [Theory]
    [InlineData("Year", "2019")]
    [InlineData("Month", "03")]
    [InlineData("Day", "05")]
    [InlineData("Hour", "07")]
    [InlineData("Minute", "08")]
    [InlineData("Second", "09")]
    [InlineData("DayOfWeek", "Tuesday")]
    public void GivenADateThenEveryNumberPartHasTwoDigitsSoTheTagsSortByDate(string part, string value)
        => Assert.Equal($":{value}", TagMask.Parse($":{{Date.{part}}}", [new FixedDateAutoTag()]).SelectMany(mask => mask.Apply(FixedAutoTag.AnyFile)).Match(left: Identity, right: tag => tag.FullName));

    private sealed class FixedDateAutoTag : DateAutoTag
    {
        public override string Name => "Date";

        public override string Description => "A fixed date.";

        protected override Option<DateTime> Date(AutoTagFile file)
            => Taken;
    }
}
