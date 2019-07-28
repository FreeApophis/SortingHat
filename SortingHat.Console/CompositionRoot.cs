using Autofac;
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
using System.Threading;
using apophis.CLI.Writer;
using apophis.FileSystem;
using SortingHat.API.AutoTag;
using SortingHat.API.Parser;

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

            RegisterCommands(Builder);
            RegisterAutoTags(Builder);
            RegisterPlugins(Builder);

            RegisterConfiguration(Builder);

            return this;
        }

        public IContainer Build()
        {


            return ConfigureLogger(Builder.Build());
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
