using System.Linq;
using Autofac;
using SortingHat.API.Events;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Plugins hear what happened to files through <see cref="IFileEventHandler{TEvent}"/>, through the real command line application.
/// </summary>
public sealed class FileEventTest
{
    [Fact]
    public void GivenAHandlerThenItHearsWhichTagsAFileGot()
    {
        using var hat = new HatSandbox(Register<TaggedFileReporter>);
        var path = hat.WriteFile("a.txt", "a");

        Assert.Equal(0, hat.Hat(["tag-files", ":x", ":y", path]));

        Assert.Contains("heard: a.txt tagged with :x, :y", hat.Lines);
    }

    [Fact]
    public void GivenSeveralFilesThenTheHandlerHearsOfEachFileOnce()
    {
        using var hat = new HatSandbox(Register<TaggedFileReporter>);
        var a = hat.WriteFile("a.txt", "a");
        var b = hat.WriteFile("b.txt", "b");

        hat.Hat(["tag-files", ":x", a, b]);

        Assert.Equal(["heard: a.txt tagged with :x", "heard: b.txt tagged with :x"], hat.Lines.Where(line => line.StartsWith("heard:")));
    }

    [Fact]
    public void GivenFilesStoredWithoutTagsThenTheHandlerHearsNothing()
    {
        using var hat = new HatSandbox(Register<TaggedFileReporter>);
        var path = hat.WriteFile("a.txt", "a");

        hat.Hat(["tag-files", path]);

        Assert.DoesNotContain(hat.Lines, line => line.StartsWith("heard:"));
    }

    [Fact]
    public void GivenAFailingHandlerThenTheFileKeepsItsTagsAndHatWarns()
    {
        using var hat = new HatSandbox(Register<FailingTaggedFileHandler>);
        var path = hat.WriteFile("a.txt", "a");

        Assert.Equal(0, hat.Hat(["tag-files", ":x", path]));

        Assert.Contains("Warning: FailingTaggedFileHandler (SortingHat.Test) failed: the handler broke", hat.Lines);
        Assert.Equal(["a.txt"], hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAFailingHandlerThenTheOtherHandlersStillHearTheEvent()
    {
        using var hat = new HatSandbox(builder =>
        {
            Register<FailingTaggedFileHandler>(builder);
            Register<TaggedFileReporter>(builder);
        });
        var path = hat.WriteFile("a.txt", "a");

        hat.Hat(["tag-files", ":x", path]);

        Assert.Contains("heard: a.txt tagged with :x", hat.Lines);
    }

    private static void Register<THandler>(ContainerBuilder builder)
        where THandler : IFileEventHandler<FileTagged>
        => builder.RegisterType<THandler>().As<IFileEventHandler<FileTagged>>();
}
