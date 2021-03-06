﻿using Autofac;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API;
using SortingHat.DB;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System;
using apophis.CLI;
using apophis.CLI.Reader;
using apophis.CLI.Writer;
using apophis.FileSystem;
using SortingHat.API.AutoTag;
using SortingHat.API.Parser;
using SortingHat.API.Plugin;
using SortingHat.CLI.Commands.Files;

namespace SortingHat.CLI
{
    internal class CompositionRoot
    {
        public ContainerBuilder Builder { get; } = new ContainerBuilder();

        public CompositionRoot Register()
        {
            Builder.RegisterModule(new ParserModule());
            Builder.RegisterModule(new SqliteDatabaseModule());
            Builder.RegisterModule(new FileSystemModule());

            Builder.RegisterType<Application>().AsSelf();
            Builder.RegisterType<ArgumentParser>().AsSelf();

            Builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>();
            Builder.RegisterType<AutoTagHandler>().As<IAutoTagHandler>();

            Builder.RegisterType<FilePathExtractor>().As<IFilePathExtractor>().SingleInstance();
            Builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
            Builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

            Builder.RegisterType<SearchQueryVisitor>().As<SearchQueryVisitor>().InstancePerDependency();

            Builder.Register(c => new HashService(SHA256.Create(), nameof(SHA256), c.Resolve<IConsoleWriter>())).As<IHashService>().SingleInstance();
            Builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            Builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            Builder.RegisterType<SystemConsoleWriter>().As<IConsoleWriter>();
            Builder.RegisterType<SystemConsoleReader>().As<IConsoleReader>();
            Builder.RegisterType<ConsoleApplicationInformationProvider>().As<IConsoleApplicationInformationProvider>();
            Builder.RegisterType<apophis.CLI.Console>().AsSelf();

            Builder.RegisterType<PluginLoader>().As<IPluginLoader>().SingleInstance();

            RegisterCommands();
            RegisterOptions();
            RegisterAutoTags();

            RegisterConfiguration();

            return this;
        }

        public IContainer Build()
        {
            return ConfigureLogger(Builder.Build());
        }

        private void RegisterAutoTags()
        {
            Builder.RegisterType<CreatedAtAutoTag>().As<IAutoTag>().SingleInstance();
            Builder.RegisterType<FolderFromRootAutoTag>().As<IAutoTag>().SingleInstance();
        }

        public IContainer ConfigureLogger(IContainer container)
        {
            // https://stackoverflow.com/questions/41414796/how-to-get-microsoft-extensions-loggingt-in-console-application-using-serilog
            var loggerFactory = container.Resolve<ILoggerFactory>();

            // File Logger
            //var context = new FileLoggerContext(AppContext.BaseDirectory, "fallback.log");
            //var settings = new FileLoggerSettings();
            //loggerFactory.AddFile(context, settings);

            // Console Logger
            //loggerFactory.AddConsole();

            return container;
        }

        private static string ConfigurationPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new NullReferenceException("Configuration path cannot be null");
        }

        private void RegisterConfiguration()
        {
            // https://github.com/dotnet/cli/issues/11545
            var configuration = ConfigurationRoot();

            Builder.Register(c => configuration.GetSection("Database").Get<DatabaseSettings>()).As<DatabaseSettings>();
        }

        private static IConfigurationRoot ConfigurationRoot()
        {
            return new ConfigurationBuilder()
              .SetBasePath(ConfigurationPath())
              .AddJsonFile("hat-config.json", false)
              .Build();
        }

        private void RegisterCommands()
        {
            Builder.RegisterType<FileOperations<ICopyFile>>().AsSelf();
            Builder.RegisterType<FileOperations<IMoveFile>>().AsSelf();

            var cliAssembly = GetType().Assembly;

            Builder.RegisterAssemblyTypes(cliAssembly)
                .Where(IsCommandClass)
                .As<ICommand>();
        }

        private bool IsCommandClass(Type type)
        {
            return type != null
                && type.Namespace != null
                && type.Namespace.Contains(nameof(Commands)) && type.Name.EndsWith("Command");
        }

        private void RegisterOptions()
        {
            var cliAssembly = GetType().Assembly;

            Builder.RegisterAssemblyTypes(cliAssembly)
                .Where(IsOption)
                .As<IOption>();
        }

        private bool IsOption(Type type)
        {
            return type != null
                && type.Namespace != null
                && type.Namespace.EndsWith(nameof(Options)) && type.Name.EndsWith("Option");
        }
    }
}
