using Funcky.Monads;
using SortingHat.API.Models;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

public class ProjectNameTest
{
    private static readonly string[] ExistingProjects = ["Default"];

    [Theory]
    [InlineData("myProject")]
    [InlineData("tax-2018")]
    [InlineData("my_project")]
    [InlineData("Steuererklärung")]
    public void GivenASafeNameThenItIsValid(string name)
        => Assert.Equal(name, ProjectName.ParseNew(name, ExistingProjects, "hat").Match(left: Identity, right: projectName => projectName.Value));

    [Theory]
    [InlineData("")]
    [InlineData("a/b")]
    [InlineData("a\b")]
    [InlineData("..")]
    [InlineData("my project")]
    [InlineData("project.db")]
    [InlineData("a:b")]
    public void GivenANameThatIsNotASafeFileNameThenItIsInvalid(string name)
        => IsInvalid(ProjectName.ParseNew(name, ExistingProjects, "hat"));

    [Fact]
    public void GivenANameLongerThan64CharactersThenItIsInvalid()
        => IsInvalid(ProjectName.ParseNew(new string('a', 65), ExistingProjects, "hat"));

    [Theory]
    [InlineData("CON")]
    [InlineData("nul")]
    [InlineData("Com1")]
    public void GivenANameWindowsReservesForADeviceThenItIsInvalid(string name)
        => IsInvalid(ProjectName.ParseNew(name, ExistingProjects, "hat"));

    [Theory]
    [InlineData("hat")]
    [InlineData("HAT")]
    public void GivenTheNameOfTheMainDatabaseThenItIsInvalid(string name)
        => IsInvalid(ProjectName.ParseNew(name, ExistingProjects, "hat"));

    [Theory]
    [InlineData("Default")]
    [InlineData("default")]
    public void GivenTheNameOfAnExistingProjectInAnyCaseThenItIsInvalid(string name)
        => IsInvalid(ProjectName.ParseNew(name, ExistingProjects, "hat"));

    private static void IsInvalid(Either<string, ProjectName> validated)
        => Assert.True(validated.Match(left: True, right: False), $"Expected an error, but the name was accepted: {validated}");
}
