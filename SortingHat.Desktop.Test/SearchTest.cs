using System.Threading.Tasks;
using Autofac;
using SortingHat.Desktop.Shell;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>Searching in the real window: the user types, the app searches the real database.</summary>
public sealed class SearchTest
{
    [Fact]
    public Task GivenTaggedFilesThenTypingATagShowsTheFilesWithIt()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox();
            await desktop.TaggedFile("2018.pdf", ":tax:2018");
            await desktop.TaggedFile("bank.pdf", ":bank");

            await desktop.Type(":tax");

            Assert.Contains("2018.pdf", desktop.Texts("Results"));
            Assert.DoesNotContain("bank.pdf", desktop.Texts("Results"));
            Assert.Equal(["1 file."], desktop.Texts("Summary"));
        });

    [Fact]
    public Task GivenAnInvalidQueryThenTheQueryBarSaysWhy()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox();

            await desktop.Type(":tax or or");

            Assert.Equal(["'or' is where a tag, 'not' or '(' is expected"], desktop.Texts("QueryProblem"));
        });

    [Fact]
    public Task GivenTheStartOfATagThenTheKnownTagsAreOffered()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox();
            await desktop.TaggedFile("2018.pdf", ":tax:2018");

            await desktop.Type(":ta");

            Assert.Equal([":tax", ":tax:2018"], desktop.Texts("Completions"));
        });

    [Fact]
    public Task GivenASelectedResultThenItsTagsAndPathAreShown()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox();
            var path = await desktop.TaggedFile("2018.pdf", ":tax:2018", ":scan");
            await desktop.Type(":tax");

            await desktop.SelectResult(0);

            Assert.Contains(":tax:2018", desktop.Texts("Details"));
            Assert.Contains(":scan", desktop.Texts("Details"));
            Assert.Contains(path, desktop.Texts("Details"));
        });

    [Fact]
    public Task GivenOpenThenTheSelectedFileIsOpened()
        => HeadlessApplication.Run(async () =>
        {
            var opener = new RecordingFileOpener();
            await using var desktop = new DesktopSandbox(builder => builder.RegisterInstance(opener).As<IFileOpener>());
            var path = await desktop.TaggedFile("2018.pdf", ":tax:2018");
            await desktop.Type(":tax");
            await desktop.SelectResult(0);

            await desktop.Click("OpenButton");

            Assert.Equal([path], opener.Opened);
        });
}
