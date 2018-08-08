using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.CLI.Commands;
using SortingHat.DB;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System;

namespace SortingHat.CLI
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            CompositionRoot().Resolve<Application>().Run(args);
        }

        private static IContainer ConfigureLogger(IContainer container)
        {
            var loggerFactory = container.Resolve<ILoggerFactory>();
            loggerFactory.AddConsole();

            return container;
        }

        private static IContainer CompositionRoot()
        {
            // Create your builder.
            var builder = new ContainerBuilder();

            builder.RegisterType<Application>().AsSelf();
            builder.RegisterType<ArgumentParser>().AsSelf();


            builder.Register(c => new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hat")).As<IDatabase>().SingleInstance();
            builder.Register(c => new HashService(SHA256.Create(), nameof(SHA256))).As<IHashService>().SingleInstance();
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            RegisterCommands(builder);


            return ConfigureLogger(builder.Build());
        }

        private static void RegisterCommands(ContainerBuilder builder)
        {
            builder.RegisterType<HelpCommand>().As<ICommand>();
            builder.RegisterType<InitCommand>().As<ICommand>();

            builder.RegisterType<ListTagsCommand>().As<ICommand>();
            builder.RegisterType<AddTagCommand>().As<ICommand>();

            builder.RegisterType<TagFileCommand>().As<ICommand>();
            builder.RegisterType<FindFilesCommand>().As<ICommand>();

            builder.RegisterType<RepairCommand>().As<ICommand>();
            builder.RegisterType<SortCommand>().As<ICommand>();
            builder.RegisterType<IdentifyCommand>().As<ICommand>();
        }
    }
}
