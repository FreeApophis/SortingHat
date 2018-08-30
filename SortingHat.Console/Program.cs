using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API;
using SortingHat.CLI.Commands;
using SortingHat.DB;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Karambolo.Extensions.Logging.File;

namespace SortingHat.CLI
{

    [ExcludeFromCodeCoverage]
    static class Program
    {
        // Could be configurable
        const LogLevel MinLogLevel = LogLevel.Trace;

        static void Main(string[] args)
        {
            CompositionRoot().Resolve<Application>().Run(args);
        }

        private static IContainer ConfigureLogger(IContainer container)
        {
            var loggerFactory = container.Resolve<ILoggerFactory>();

            // File Logger
            var context = new FileLoggerContext(AppContext.BaseDirectory, "fallback.log");
            var settings = new FileLoggerSettings();
            loggerFactory.AddFile(context, settings);

            // Console Logger
            //loggerFactory.AddConsole();

            return container;
        }

        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Application>().AsSelf();
            builder.RegisterType<ArgumentParser>().AsSelf();

            builder.RegisterType<SQLiteDB>().As<IDatabase>().As<SQLiteDB>().SingleInstance();
            builder.RegisterType<SQLiteFile>().As<IFile>().SingleInstance();
            builder.RegisterType<SQLiteTag>().As<ITag>().SingleInstance();

            builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>().SingleInstance();
            builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
            builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

            builder.RegisterType<SearchQueryVisitor>().As<SearchQueryVisitor>().InstancePerDependency();


            builder.Register(c => new HashService(SHA256.Create(), nameof(SHA256))).As<IHashService>().SingleInstance();
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
            RegisterConfiguration(builder);

            RegisterCommands(builder);

            return ConfigureLogger(builder.Build());
        }

        private static string ConfigurationPath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private static void RegisterConfiguration(ContainerBuilder builder)
        {
            IConfigurationRoot configuration = ConfigurationRoot();

            builder.Register(c => configuration.GetSection("Database").Get<DatabaseSettings>()).As<DatabaseSettings>();
        }

        private static IConfigurationRoot ConfigurationRoot()
        {
            return new ConfigurationBuilder()
              .SetBasePath(ConfigurationPath())
              .AddJsonFile("hat-config.json", false)
              .Build();
        }

        private static void RegisterCommands(ContainerBuilder builder)
        {
            builder.RegisterType<HelpCommand>().As<ICommand>();
            builder.RegisterType<VersionCommand>().As<ICommand>();
            builder.RegisterType<InitCommand>().As<ICommand>();

            builder.RegisterType<AddTagCommand>().As<ICommand>();
            builder.RegisterType<ListTagsCommand>().As<ICommand>();
            builder.RegisterType<RemoveTagCommand>().As<ICommand>();
            builder.RegisterType<RenameTagCommand>().As<ICommand>();
            builder.RegisterType<MoveTagCommand>().As<ICommand>();


            builder.RegisterType<TagFileCommand>().As<ICommand>();
            builder.RegisterType<UntagFileCommand>().As<ICommand>();
            builder.RegisterType<FindFilesCommand>().As<ICommand>();
            builder.RegisterType<FileInfoCommand>().As<ICommand>();
            builder.RegisterType<MoveFilesCommand>().As<ICommand>();
            builder.RegisterType<CopyFilesCommand>().As<ICommand>();
            builder.RegisterType<DuplicateFileCommand>().As<ICommand>();
            


            builder.RegisterType<RepairCommand>().As<ICommand>();
            builder.RegisterType<IdentifyCommand>().As<ICommand>();
        }
    }
}
