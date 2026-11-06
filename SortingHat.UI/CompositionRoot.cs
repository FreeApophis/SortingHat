using System.Security.Cryptography;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Parser;
using SortingHat.DB;

namespace SortingHat.UI;

internal class CompositionRoot
{
    // Could be configurable
    private const LogLevel MinLogLevel = LogLevel.Trace;

    public IContainer Build()
    {
        var builder = new ContainerBuilder();

        builder.RegisterModule(new ParserModule());
        builder.RegisterModule(new SqliteDatabaseModule());
        builder.RegisterModule(new FileSystemModule());

        builder.RegisterType<MainWindow>().AsSelf();

        builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>().SingleInstance();
        builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
        builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

        builder.Register(_ => SHA256.Create()).As<HashAlgorithm>().SingleInstance();
        builder.Register(c => new HashService(c.Resolve<HashAlgorithm>(), nameof(SHA256), c.Resolve<IConsoleWriter>())).As<IHashService>().SingleInstance();
        builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
        builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
        builder.RegisterType<NullWriter>().As<IConsoleWriter>();

        builder.Register(c => new DatabaseSettings { Type = "sqlite", Name = "hat", Path = "#USERDOC", DefaultProject = "Default" });

        return ConfigureLogger(builder.Build());
    }

    private IContainer ConfigureLogger(IContainer container)
    {
        var loggerFactory = container.Resolve<ILoggerFactory>();

        // loggerFactory.AddDebug(MinLogLevel);
#pragma warning disable IDISP004 // The logger factory takes ownership of its providers and disposes them.
        loggerFactory.AddProvider(new WindowLoggerProvider());
#pragma warning restore IDISP004

        return container;
    }
}
