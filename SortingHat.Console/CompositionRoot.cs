using System;
using System.IO;
using System.Security.Cryptography;
using Apophis.CLI;
using Apophis.CLI.Reader;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Parser;
using SortingHat.API.Plugin;
using SortingHat.CLI.Commands.Files;
using SortingHat.DB;

namespace SortingHat.CLI;

internal class CompositionRoot
{
    public ContainerBuilder Builder { get; } = new ContainerBuilder();

    public CompositionRoot Register()
    {
        Builder.RegisterModule(new ParserModule());
        Builder.RegisterModule(new SqliteDatabaseModule());
        Builder.RegisterModule(new FileSystemModule());

        Builder.RegisterType<Application>().AsSelf();
        Builder.RegisterType<CommandLine>().AsSelf();

        Builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>();
        Builder.RegisterType<AutoTagHandler>().As<IAutoTagHandler>();

        Builder.RegisterType<FilePathExtractor>().As<IFilePathExtractor>().SingleInstance();
        Builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
        Builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

        Builder.Register(_ => SHA256.Create()).As<HashAlgorithm>().InstancePerDependency().ExternallyOwned();
        Builder.Register(c => new HashService(c.Resolve<Func<HashAlgorithm>>(), nameof(SHA256), c.Resolve<IConsoleWriter>())).As<IHashService>().SingleInstance();
        Builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
        Builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

        Builder.RegisterType<SystemConsoleWriter>().As<IConsoleWriter>();
        Builder.RegisterType<SystemConsoleReader>().As<IConsoleReader>();
        Builder.RegisterType<ConsoleApplicationInformationProvider>().As<IConsoleApplicationInformationProvider>();
        Builder.RegisterType<Apophis.CLI.Console>().AsSelf();

        Builder.RegisterType<PluginLoader>().As<IPluginLoader>().SingleInstance();
        Builder.RegisterInstance(new PluginDirectory(Path.Combine(AppContext.BaseDirectory, "plugins")));
        Builder.RegisterInstance(ContractAssemblies.Current);

        RegisterCommands();
        RegisterAutoTags();

        RegisterConfiguration();

        return this;
    }

    public IContainer Build()
        => Builder.Build();

    /// <summary>The scope the application runs in: the <paramref name="container"/> with every plugin's modules registered.</summary>
    public static ILifetimeScope BeginPluginScope(IContainer container)
        => container.BeginLifetimeScope(builder => container.Resolve<IPluginLoader>().RegisterModules(builder));

    private void RegisterAutoTags()
    {
        Builder.RegisterType<CreatedAtAutoTag>().As<IAutoTag>().SingleInstance();
        Builder.RegisterType<FolderFromRootAutoTag>().As<IAutoTag>().SingleInstance();
    }

    private void RegisterConfiguration()
    {
        // https://github.com/dotnet/cli/issues/11545
        var configuration = ConfigurationRoot();

        Builder.Register(c => configuration.GetSection("Database").Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("The 'Database' section is missing in hat-config.json")).As<DatabaseSettings>();
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

        // every built-in command is an IDeclaredCommand, only plugins may still register an ICommand
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
