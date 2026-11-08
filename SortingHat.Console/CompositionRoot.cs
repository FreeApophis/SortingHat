using System;
using System.Collections.Generic;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Autofac;
using Funcky.Monads;
using Microsoft.Extensions.Configuration;
using Serilog;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using SortingHat.CLI.Commands.Files;
using SortingHat.DB;
using Spectre.Console;

namespace SortingHat.CLI;

internal class CompositionRoot
{
    public ContainerBuilder Builder { get; } = new ContainerBuilder();

    public CompositionRoot Register()
    {
        Builder.RegisterModule(new CoreModule());
        Builder.RegisterModule(new SqliteDatabaseModule());
        Builder.RegisterModule(new FileSystemModule());

        Builder.RegisterType<Application>().AsSelf();
        Builder.RegisterType<CommandLine>().AsSelf();

        // the logger is disposed with the container, which writes what is left
        Builder.Register(context => HatLog.Create(context.Resolve<DatabaseSettings>(), context.Resolve<LogSettings>())).As<ILogger>().SingleInstance();
        Builder.RegisterModule<LoggerModule>();

        Builder.Register(_ => HatConsole.Create()).As<IAnsiConsole>().SingleInstance();
        Builder.RegisterType<PlainLineWriter>().As<IConsoleWriter>().SingleInstance();
        Builder.RegisterType<FileProgress>().AsSelf().SingleInstance();
        Builder.RegisterType<SpectreUserPrompt>().As<IUserPrompt>().SingleInstance();
        Builder.RegisterType<ApplicationName>().AsSelf().SingleInstance();

        RegisterCommands();

        RegisterConfiguration();

        return this;
    }

    public IContainer Build()
        => Builder.Build();

    /// <summary>The scope the application runs in: the <paramref name="container"/> with every plugin's modules registered.</summary>
    public static ILifetimeScope BeginPluginScope(IContainer container)
        => container.BeginLifetimeScope(builder =>
        {
            builder.RegisterModule<LoggerModule>();
            container.Resolve<IPluginLoader>().RegisterModules(builder);
        });

    private void RegisterConfiguration()
    {
        // https://github.com/dotnet/cli/issues/11545
        var configuration = ConfigurationRoot();

        Builder.Register(c => configuration.GetSection("Database").Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("The 'Database' section is missing in hat-config.json")).As<DatabaseSettings>();

        Builder.Register(_ => Option.FromNullable(configuration.GetSection("Log").Get<LogSettings>()).GetOrElse(new LogSettings())).As<LogSettings>();
    }

    private static IConfigurationRoot ConfigurationRoot()
    {
        // the folder of hat.exe, also in a single-file bundle where Assembly.Location is empty
        return new ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddJsonFile("hat-config.json", false)
          .Build();
    }

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
