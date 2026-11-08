using System;
using System.IO;
using Autofac;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// hat opens a database only when a command uses it, through the real command line application with real SQLite databases.
/// A database file that does not exist yet is not created by commands that do not need it.
/// </summary>
public sealed class LazyDatabaseTest
{
    [Theory]
    [InlineData("help")]
    [InlineData("version")]
    [InlineData("plugins")]
    [InlineData("tag-files", "--help")]
    public void GivenACommandWithoutDataThenHatCreatesNoDatabase(params string[] arguments)
    {
        using var hat = new HatSandbox();

        Assert.Equal(0, hat.Hat(arguments));

        Assert.False(File.Exists(hat.ProjectDatabaseFile("hat")));
        Assert.False(File.Exists(hat.ProjectDatabaseFile("Default")));
    }

    [Fact]
    public void GivenListProjectsThenOnlyTheMainDatabaseIsOpened()
    {
        using var hat = new HatSandbox();

        Assert.Equal(0, hat.Hat(["list-projects"]));

        Assert.True(File.Exists(hat.ProjectDatabaseFile("hat")));
        Assert.False(File.Exists(hat.ProjectDatabaseFile("Default")));
    }

    [Fact]
    public void GivenAFailingMigrationThenTheDatabaseFileIsReleased()
    {
        using var hat = new HatSandbox(builder => builder.RegisterInstance<ICodeMigration>(
            new CodeMigration("29990101-0000-AlwaysFails", MigrationType.Project, _ => throw new InvalidOperationException("the migration broke"))));
        var path = hat.WriteFile("a.txt", "a");

        Assert.NotEqual(0, hat.Hat(["tag-files", ":x", path]));

        Assert.Contains("the migration broke", hat.Lines);
        File.Delete(hat.ProjectDatabaseFile("Default"));
    }
}
