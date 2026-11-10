using System;
using Apophis.FileSystem;
using Autofac;
using Funcky.Monads;
using Microsoft.Extensions.Configuration;
using Serilog;
using SortingHat.API;
using SortingHat.App.Files;
using SortingHat.App.Tags;
using SortingHat.DB;

namespace SortingHat.App;

/// <summary>
/// Registers what every host of SortingHat shares: the core, the SQLite database, the file system, hat-config.json,
/// the log and the workflows. A host adds only what is its own, such as the console or the window.
/// </summary>
public sealed class HostModule : Module
{
    private const string ConfigurationFile = "hat-config.json";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CoreModule>();
        builder.RegisterModule<SqliteDatabaseModule>();
        builder.RegisterModule<FileSystemModule>();

        builder.Register(_ => Configuration()).As<IConfiguration>().SingleInstance();
        builder.Register(context => DatabaseSettings(context.Resolve<IConfiguration>())).As<DatabaseSettings>();
        builder.Register(context => LogSettings(context.Resolve<IConfiguration>())).As<LogSettings>();

        // the logger is disposed with the container, which writes what is left
        builder.Register(context => HatLog.Create(context.Resolve<DatabaseSettings>(), context.Resolve<LogSettings>())).As<ILogger>().SingleInstance();
        builder.RegisterModule<LoggerModule>();

        builder.RegisterType<StartupReport>().AsSelf();
        builder.RegisterType<FileReports>().AsSelf();
        builder.RegisterType<TagEdits>().AsSelf();
    }

    /// <summary>hat-config.json next to the executable, also in a single-file bundle where Assembly.Location is empty.</summary>
    private static IConfiguration Configuration()
        => new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(ConfigurationFile, optional: false)
            .Build();

    private static DatabaseSettings DatabaseSettings(IConfiguration configuration)
        => Option.FromNullable(configuration.GetSection("Database").Get<DatabaseSettings>())
            .GetOrElse(() => throw new InvalidOperationException($"The 'Database' section is missing in {ConfigurationFile}"));

    private static LogSettings LogSettings(IConfiguration configuration)
        => Option.FromNullable(configuration.GetSection("Log").Get<LogSettings>()).GetOrElse(new LogSettings());
}
