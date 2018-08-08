using Autofac;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Parser;
using SortingHat.DB;
using System.Security.Cryptography;
using System;

namespace SortingHat.CLI
{
    class Program
    {
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            try
            {
                RegisterServices();
                ConfigureLogger();

                var argumentParser = new ArgumentParser(args, Container);

                argumentParser.Execute();
            }
            catch (ParseException e)
            {
                Console.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("no such table:"))
                {
                    Console.WriteLine("Database not initialized? Run .hat init");
                }
                else
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(-1);
                }
            }
        }

        private static void ConfigureLogger()
        {
            var loggerFactory = Container.Resolve<ILoggerFactory>();
            loggerFactory.AddConsole();
        }

        static void RegisterServices()
        {
            // Create your builder.
            var builder = new ContainerBuilder();
            builder.RegisterType<SQLiteDB>().As<IDatabase>().SingleInstance();
            var hashAlgorithm = new TypedParameter(typeof(HashAlgorithm), SHA256.Create());
            var hashPrefix = new TypedParameter(typeof(string), nameof(SHA256));
            builder.RegisterType<HashService>().AsSelf().WithParameter(hashAlgorithm).WithParameter(hashPrefix).SingleInstance();
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            Container = builder.Build();
        }
    }
}
