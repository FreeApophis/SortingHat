using System;
using System.Collections.Generic;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Autofac;
using SortingHat.API.DI;
using SortingHat.App;
using SortingHat.CLI.Commands.Files;
using Spectre.Console;

namespace SortingHat.CLI;

internal class CompositionRoot
{
    public ContainerBuilder Builder { get; } = new ContainerBuilder();

    public CompositionRoot Register()
    {
        Builder.RegisterModule<HostModule>();

        Builder.RegisterType<Application>().AsSelf();
        Builder.RegisterType<CommandLine>().AsSelf();

        Builder.Register(_ => HatConsole.Create()).As<IAnsiConsole>().SingleInstance();
        Builder.RegisterType<PlainLineWriter>().As<IConsoleWriter>().SingleInstance();
        Builder.RegisterType<FileProgress>().AsSelf().SingleInstance();
        Builder.RegisterType<SpectreUserPrompt>().As<IUserPrompt>().SingleInstance();
        Builder.RegisterType<ApplicationName>().AsSelf().SingleInstance();

        RegisterCommands();

        return this;
    }

    public IContainer Build()
        => Builder.Build();

    private void RegisterCommands()
    {
        Builder.RegisterType<FileOperations<ICopyFile>>().AsSelf();
        Builder.RegisterType<FileOperations<IMoveFile>>().AsSelf();

        // once per run of hat, with the commands of the plugins
        Builder.Register(context => CommandNames.Assign(context.Resolve<IEnumerable<IDeclaredCommand>>())).AsSelf().InstancePerLifetimeScope();

        Builder.RegisterAssemblyTypes(GetType().Assembly)
            .Where(type => IsCommandClass(type) && type.IsAssignableTo<IDeclaredCommand>())
            .As<IDeclaredCommand>();
    }

    private bool IsCommandClass(Type type)
    {
        return type != null
            && type.Namespace != null
            && type.Namespace.Contains(nameof(Commands)) && type.Name.EndsWith("Command");
    }
}
