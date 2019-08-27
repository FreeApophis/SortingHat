using System;
using System.Linq;
using System.Reflection;
using apophis.CLI;
using apophis.CLI.Writer;
using Autofac;
using Moq;
using SortingHat.API.DI;
using SortingHat.CLI;
using SortingHat.Test.Mock;
using Xunit;

namespace SortingHat.Test.Commands
{
    public class HelpCommandTest
    {
        [Fact]
        public void GivenNoCommandThenGiveHint()
        {
            var writer = new MemoryConsoleWriter();

            var application = CompositionRootWithMyWriter(writer);

            // Run application without a command
            application.Execute(new string[] { });

            // Test the output
            Assert.Collection(writer.Lines, line => Assert.Equal("Maybe run 'program help'", line));
        }

        [Fact]
        public void GivenTheHelpCommandWithNoArgumentReturnTheDefaultHelp()
        {
            var writer = new MemoryConsoleWriter();

            var application = CompositionRootWithMyWriter(writer);

            // Run application without a command
            application.Execute(new[] { "help" });

            // Test the output
            Assert.Equal("program <command> [<arguments>] [<options>]:", writer.Lines[0]);
        }

        [Theory]
        [MemberData(nameof(HelpCommands))]
        public void GivenTheHelpWithACommandArgumentShowTheCorrespondingHelp(string command)
        {
            var writer = new MemoryConsoleWriter();
            var application = CompositionRootWithMyWriter(writer);

            // Run application without a command
            Assert.True(application.Execute(new[] { "help", command }));

            Assert.NotEmpty(writer.Lines[0]);
        }

        public static TheoryData<string> HelpCommands()
        {

            return new TheoryData<string>
            {
                "help",
                "plugins",
                "repair",
                "statistics",
                "version",
//                "exif",
//                "identify",
                "add-tags",
                "list-tags",
                "move-tags",
                "remove-tags",
                "rename-tag",
                "copy-files",
                "duplicate",
                "file-info",
                "find-files",
                "move-files",
                "tag-files",
                "untag-files",
//                "scan",
                "new-project",
                "remove-project",
                "export",
                "import",
                "list-projects",
                "switch-project"
            };
        }

        private static string ExtractCommandName(Type arg)
        {
            throw new NotImplementedException();
        }

        private static bool IsCommandClass(Type t)
        {
            return t.Namespace != null && (t.IsClass && t.Namespace.Contains("Commands") && t.Name.EndsWith("Command"));
        }

        private static ArgumentParser CompositionRootWithMyWriter(MemoryConsoleWriter writer)
        {
            // CLI Application CompositionRoot
            var compositionRoot = new CompositionRoot()
                .Register();

            // We override the Console Output to the Memory Writer
            compositionRoot.Builder.Register(context => writer).As<IConsoleWriter>();

            OverrideDatabase(compositionRoot);
            OverrideConsoleApplicationInformationProvider(compositionRoot);

            // Return The application Object
            return compositionRoot
                .Build()
                .Resolve<ArgumentParser>();
        }

        private static void OverrideConsoleApplicationInformationProvider(CompositionRoot compositionRoot)
        {
            var mock = new Mock<IConsoleApplicationInformationProvider>(MockBehavior.Strict);

            mock.Setup(m => m.Name).Returns("program");

            compositionRoot.Builder.Register(context => mock.Object).As<IConsoleApplicationInformationProvider>();
        }

        private static void OverrideDatabase(CompositionRoot compositionRoot)
        {
            // Make sure we do not use the database module
            compositionRoot.Builder.Register(context => MockProjectDatabase.Create()).As<IProjectDatabase>().InstancePerLifetimeScope();
            compositionRoot.Builder.Register(context => MockMainDatabase.Create()).As<IMainDatabase>().InstancePerLifetimeScope();
        }
    }
}
