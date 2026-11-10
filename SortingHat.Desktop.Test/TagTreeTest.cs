using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>The tag tree in the real window, on the real database.</summary>
public sealed class TagTreeTest
{
    [AvaloniaFact]
    public async Task GivenTaggedFilesThenTheTreeShowsTheTagsWithTheirFileCounts()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");
        await desktop.TaggedFile("b.pdf", ":tax:2018");

        await desktop.Idle();

        Assert.Equal(["tax", "0", "2018", "2"], desktop.Texts("TagTree"));
    }

    [AvaloniaFact]
    public async Task GivenAChosenTagThenItsFilesAreFound()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");
        await desktop.TaggedFile("b.pdf", ":bank");

        await desktop.ChooseTag(":tax:2018");

        Assert.Contains("a.pdf", desktop.Texts("Results"));
        Assert.DoesNotContain("b.pdf", desktop.Texts("Results"));
    }

    [AvaloniaFact]
    public async Task GivenARenamedTagThenTheTreeShowsTheNewName()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");

        await desktop.TagMenu(":tax:2018", "Rename…");
        await desktop.Enter("TagEditText", "2019");
        await desktop.Click("TagEditConfirm");

        Assert.Equal(["tax", "0", "2019", "1"], desktop.Texts("TagTree"));
        Assert.Contains("Renamed ':tax:2018' to ':tax:2019'.", desktop.Texts("MessagePanel"));
    }

    [AvaloniaFact]
    public async Task GivenAMovedTagThenItIsUnderItsNewParent()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");
        await desktop.TaggedFile("b.pdf", ":archive");

        await desktop.TagMenu(":tax:2018", "Move…");
        await desktop.Enter("TagEditText", ":archive");
        await desktop.Click("TagEditConfirm");

        Assert.Equal(["archive", "1", "2018", "1", "tax", "0"], desktop.Texts("TagTree"));
    }

    [AvaloniaFact]
    public async Task GivenARemovedTagThenTheTreeNoLongerShowsIt()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":bank");
        await desktop.TaggedFile("b.pdf", ":tax");

        await desktop.TagMenu(":bank", "Remove…");
        await desktop.Click("TagEditConfirm");

        Assert.Equal(["tax", "1"], desktop.Texts("TagTree"));
    }

    [AvaloniaFact]
    public async Task GivenATagWithChildrenThenRemovingItSaysWhyAndKeepsIt()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");

        await desktop.TagMenu(":tax", "Remove…");
        await desktop.Click("TagEditConfirm");

        Assert.Equal(["The tag ':tax' does not exist or still has child tags."], desktop.Texts("TagEditProblem"));
        Assert.Equal(["tax", "0", "2018", "1"], desktop.Texts("TagTree"));
    }

    [AvaloniaFact]
    public async Task GivenARenamedTagThenTheDetailsOfTheSelectedFileShowIt()
    {
        await using var desktop = new DesktopSandbox();
        await desktop.TaggedFile("a.pdf", ":tax:2018");
        await desktop.Type(":tax");
        await desktop.SelectResult(0);

        await desktop.TagMenu(":tax:2018", "Rename…");
        await desktop.Enter("TagEditText", "2019");
        await desktop.Click("TagEditConfirm");

        Assert.Contains(":tax:2019", desktop.Texts("Details"));
    }
}
