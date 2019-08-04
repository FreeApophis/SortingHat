using apophis.CLI.Writer;
using Autofac;
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
            application.Run(new string[] { });

            // Test the output
            Assert.Collection(writer.Lines, line => Assert.Equal("Maybe run 'hat help'", line));
        }

        [Fact]
        public void GivenTheHelpCommandWithNoArgumentReturnTheDefaultHelp()
        {
            var writer = new MemoryConsoleWriter();

            var application = CompositionRootWithMyWriter(writer);

            // Run application without a command
            application.Run(new[] { "help" });

            // Test the output
            Assert.Equal("hat <command> [<arguments>] [<options>]:", writer.Lines[0]);
        }

        private static Application CompositionRootWithMyWriter(MemoryConsoleWriter writer)
        {
            // CLI Application CompositionRoot
            var compositionRoot = new CompositionRoot()
                .Register();

            // We override the Console Output to the Memory Writer
            compositionRoot.Builder.Register(context => writer).As<IConsoleWriter>();

            OverrideDatabase(compositionRoot);

            // Return The application Object
            return compositionRoot
                .Build()
                .Resolve<Application>();
        }

        private static void OverrideDatabase(CompositionRoot compositionRoot)
        {
            // Make sure we do not use the database module
            compositionRoot.Builder.Register(context => MockProjectDatabase.Create()).As<IProjectDatabase>().InstancePerLifetimeScope();
            compositionRoot.Builder.Register(context => MockMainDatabase.Create()).As<IMainDatabase>().InstancePerLifetimeScope();
        }
    }
}
