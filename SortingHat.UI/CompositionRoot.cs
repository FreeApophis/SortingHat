using System.Security.Cryptography;
using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB;

namespace SortingHat.UI
{
    internal class CompositionRoot
    {
        // Could be configurable
        const LogLevel MinLogLevel = LogLevel.Trace;

        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MainWindow>().AsSelf();

            builder.RegisterType<SQLiteDB>().As<IDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<SQLiteFile>().As<IFile>().SingleInstance();
            builder.RegisterType<SQLiteTag>().As<ITag>().SingleInstance();

            builder.RegisterType<API.Models.TagParser>().As<API.Models.ITagParser>().SingleInstance();
            builder.RegisterType<API.Models.Tag>().As<API.Models.Tag>().InstancePerDependency();
            builder.RegisterType<API.Models.File>().As<API.Models.File>().InstancePerDependency();

            builder.RegisterType<SearchQueryVisitor>().As<SearchQueryVisitor>().InstancePerDependency();


            builder.Register(c => new HashService(SHA256.Create(), nameof(SHA256))).As<IHashService>().SingleInstance();
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            builder.Register(c => new DatabaseSettings { DBType = "sqlite", DBName = "hat", DBPath = "#USERDOC" });

            return ConfigureLogger(builder.Build());
        }

        private IContainer ConfigureLogger(IContainer container)
        {
            var loggerFactory = container.Resolve<ILoggerFactory>();
            //loggerFactory.AddDebug(MinLogLevel);
            loggerFactory.AddProvider(new WindowLoggerProvider());

            return container;
        }
    }
}
