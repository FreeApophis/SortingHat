﻿using Autofac;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using SortingHat.API;
using SortingHat.DB;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System;
using SortingHat.API.AutoTag;
using SortingHat.API.Parser;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI
{
    internal class CompositionRoot
    {
        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ParserModule());
            builder.RegisterModule(new SqliteDatabaseModule());

            builder.RegisterType<Application>().AsSelf();
            builder.RegisterType<ArgumentParser>().AsSelf();

            builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>();
            builder.RegisterType<AutoTagHandler>().As<IAutoTagHandler>();

            builder.RegisterType<FilePathExtractor>().As<IFilePathExtractor>().SingleInstance();
            builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
            builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

            builder.RegisterType<SearchQueryVisitor>().As<SearchQueryVisitor>().InstancePerDependency();

            builder.Register(c => new HashService(SHA256.Create(), nameof(SHA256), c.Resolve<IConsoleWriter>())).As<IHashService>().SingleInstance();
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
            builder.RegisterType<SystemConsoleWriter>().As<IConsoleWriter>();

            RegisterConfiguration(builder);
            RegisterCommands(builder);
            RegisterAutoTags(builder);
            RegisterPlugins(builder);

            return ConfigureLogger(builder.Build());
        }

        private void RegisterAutoTags(ContainerBuilder builder)
        {
            builder.RegisterType<CreatedAtAutoTag>().As<IAutoTag>().SingleInstance();
            builder.RegisterType<FolderFromRootAutoTag>().As<IAutoTag>().SingleInstance();
        }

        public IContainer ConfigureLogger(IContainer container)
        {
            // https://stackoverflow.com/questions/41414796/how-to-get-microsoft-extensions-loggingt-in-console-application-using-serilog
            var loggerFactory = container.Resolve<ILoggerFactory>();

            // File Logger
            var context = new FileLoggerContext(AppContext.BaseDirectory, "fallback.log");
            var settings = new FileLoggerSettings();
            loggerFactory.AddFile(context, settings);

            // Console Logger
            //loggerFactory.AddConsole();

            return container;
        }

        private void RegisterPlugins(ContainerBuilder builder)
        {
            // Instantiate the plugin loader
            IPluginLoader pluginLoader = new PluginLoader();

            // Make the plugin loader available in the IoC container
            builder.RegisterInstance(pluginLoader).As<IPluginLoader>().SingleInstance();

            // Register plugin modules
            pluginLoader.RegisterModules(builder);
        }

        private static string ConfigurationPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new NullReferenceException("Configuration path cannot be null");
        }

        private void RegisterConfiguration(ContainerBuilder builder)
        {
            // https://github.com/dotnet/cli/issues/11545
            var configuration = ConfigurationRoot();

            builder.Register(c => configuration.GetSection("Database").Get<DatabaseSettings>()).As<DatabaseSettings>();
        }

        private static IConfigurationRoot ConfigurationRoot()
        {
            return new ConfigurationBuilder()
              .SetBasePath(ConfigurationPath())
              .AddJsonFile("hat-config.json", false)
              .Build();
        }

        private void RegisterCommands(ContainerBuilder builder)
        {
            var cliAssembly = GetType().Assembly;

            builder.RegisterAssemblyTypes(cliAssembly)
                .Where(IsCommandClass)
                .As<ICommand>();
        }

        private bool IsCommandClass(Type type)
        {
            return type != null
                && type.Namespace != null
                && type.Namespace.Contains(nameof(Commands)) && type.Name.EndsWith("Command");
        }
    }
}
