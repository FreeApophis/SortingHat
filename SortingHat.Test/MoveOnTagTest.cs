using System;
using System.IO;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The MoveOnTag plugin, loaded from 'plugins' next to hat, through the real command line application:
/// a file that gets a tag with a move rule is moved into the folder of the rule.
/// </summary>
public sealed class MoveOnTagTest : IDisposable
{
    private readonly HatSandbox _hat = HatSandbox.WithPluginFixtures("Working");

    [Fact]
    public void GivenARuleThenTaggingAFileMovesItIntoTheFolder()
    {
        var tax = _hat.CreateFolder("tax");
        var path = _hat.WriteFile("a.txt", "a");
        Assert.Equal(0, _hat.Hat(["move-rule", ":tax", tax]));

        Assert.Equal(0, _hat.Hat(["tag-files", ":tax", path]));

        Assert.False(File.Exists(path));
        Assert.True(File.Exists(Path.Combine(tax, "a.txt")));
    }

    [Fact]
    public void GivenAMovedFileThenTheDatabaseKnowsItsNewPath()
    {
        var tax = _hat.CreateFolder("tax");
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Hat(["move-rule", ":tax", tax]);

        _hat.Hat(["tag-files", ":tax", path]);

        _hat.Hat(["find-files", ":tax"]);
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith(Path.Combine(tax, "a.txt")));
        Assert.DoesNotContain(_hat.Lines, line => line.TrimEnd().EndsWith(path));
    }

    [Fact]
    public void GivenARuleForAParentTagThenAChildTagMovesTheFile()
    {
        var tax = _hat.CreateFolder("tax");
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Hat(["move-rule", ":tax", tax]);

        _hat.Hat(["tag-files", ":tax:2018", path]);

        Assert.True(File.Exists(Path.Combine(tax, "a.txt")));
    }

    [Fact]
    public void GivenRulesForATagAndItsChildThenTheMostSpecificRuleWins()
    {
        var tax = _hat.CreateFolder("tax");
        var tax2018 = _hat.CreateFolder("tax2018");
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Hat(["move-rule", ":tax", tax]);
        _hat.Hat(["move-rule", ":tax:2018", tax2018]);

        _hat.Hat(["tag-files", ":tax:2018", path]);

        Assert.True(File.Exists(Path.Combine(tax2018, "a.txt")));
    }

    [Fact]
    public void GivenTwoEquallySpecificRulesThenTheFileStaysAndHatSaysWhy()
    {
        var tax = _hat.CreateFolder("tax");
        var bank = _hat.CreateFolder("bank");
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Hat(["move-rule", ":tax", tax]);
        _hat.Hat(["move-rule", ":bank", bank]);

        _hat.Hat(["tag-files", ":tax", ":bank", path]);

        Assert.True(File.Exists(path));
        Assert.Contains(_hat.Lines, line => line.Contains("not moved") && line.Contains(":tax") && line.Contains(":bank"));
    }

    [Fact]
    public void GivenAFileWithTheSameNameInTheFolderThenTheFileStaysAndHatSaysWhy()
    {
        var tax = _hat.CreateFolder("tax");
        File.WriteAllText(Path.Combine(tax, "a.txt"), "another file");
        var path = _hat.WriteFile("a.txt", "a");
        _hat.Hat(["move-rule", ":tax", tax]);

        _hat.Hat(["tag-files", ":tax", path]);

        Assert.True(File.Exists(path));
        Assert.Equal("another file", File.ReadAllText(Path.Combine(tax, "a.txt")));
        Assert.Contains(_hat.Lines, line => line.Contains("not moved") && line.Contains("already exists"));
    }

    [Fact]
    public void GivenTagsWithoutARuleThenTheFileStays()
    {
        _hat.Hat(["move-rule", ":tax", _hat.CreateFolder("tax")]);
        var path = _hat.WriteFile("a.txt", "a");

        _hat.Hat(["tag-files", ":bank", path]);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void GivenARuleInOneProjectThenAnotherProjectHasNoRules()
    {
        _hat.Hat(["move-rule", ":tax", _hat.CreateFolder("tax")]);
        _hat.Hat(["new-project", "other"]);
        _hat.Hat(["switch-project", "other"]);

        _hat.Hat(["move-rules"]);

        Assert.Equal(["There are no move rules."], _hat.Lines);
    }

    [Fact]
    public void GivenARuleInAnotherProjectThenTaggingDoesNotMoveTheFile()
    {
        _hat.Hat(["move-rule", ":tax", _hat.CreateFolder("tax")]);
        _hat.Hat(["new-project", "other"]);
        _hat.Hat(["switch-project", "other"]);
        var path = _hat.WriteFile("a.txt", "a");

        _hat.Hat(["tag-files", ":tax", path]);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void GivenRulesThenMoveRulesListsThem()
    {
        var tax = _hat.CreateFolder("tax");
        _hat.Hat(["move-rule", ":tax", tax]);

        Assert.Equal(0, _hat.Hat(["move-rules"]));

        Assert.Equal([$":tax -> {tax}"], _hat.Lines);
    }

    [Fact]
    public void GivenNoRulesThenMoveRulesSaysSo()
    {
        Assert.Equal(0, _hat.Hat(["move-rules"]));

        Assert.Equal(["There are no move rules."], _hat.Lines);
    }

    [Fact]
    public void GivenARuleForTheSameTagThenItReplacesTheFolder()
    {
        _hat.Hat(["move-rule", ":tax", _hat.CreateFolder("old")]);
        var tax = _hat.CreateFolder("tax");

        _hat.Hat(["move-rule", ":tax", tax]);

        _hat.Hat(["move-rules"]);
        Assert.Equal([$":tax -> {tax}"], _hat.Lines);
    }

    [Fact]
    public void GivenRemoveThenTheRuleIsGone()
    {
        _hat.Hat(["move-rule", ":tax", _hat.CreateFolder("tax")]);

        Assert.Equal(0, _hat.Hat(["move-rule", "--remove", ":tax"]));

        _hat.Hat(["move-rules"]);
        Assert.Equal(["There are no move rules."], _hat.Lines);
    }

    [Fact]
    public void GivenRemoveOfAnUnknownRuleThenHatReportsIt()
    {
        Assert.NotEqual(0, _hat.Hat(["move-rule", "--remove", ":tax"]));

        Assert.Equal(["There is no move rule for ':tax'."], _hat.Lines);
    }

    [Fact]
    public void GivenAMissingFolderThenNoRuleIsAdded()
    {
        var missing = Path.Combine(_hat.CreateFolder("somewhere"), "missing");

        Assert.NotEqual(0, _hat.Hat(["move-rule", ":tax", missing]));

        Assert.Contains(_hat.Lines, line => line.Contains("no folder"));
        _hat.Hat(["move-rules"]);
        Assert.Equal(["There are no move rules."], _hat.Lines);
    }

    [Fact]
    public void GivenAnInvalidTagThenNoRuleIsAdded()
    {
        Assert.NotEqual(0, _hat.Hat(["move-rule", ":tax 2018", _hat.CreateFolder("tax")]));

        _hat.Hat(["move-rules"]);
        Assert.Equal(["There are no move rules."], _hat.Lines);
    }

    [Fact]
    public void GivenNeitherAFolderNorRemoveThenMoveRuleAsksForTheFolder()
    {
        Assert.NotEqual(0, _hat.Hat(["move-rule", ":tax"]));

        Assert.Contains(_hat.Lines, line => line.Contains("folder"));
    }

    public void Dispose()
        => _hat.Dispose();
}
