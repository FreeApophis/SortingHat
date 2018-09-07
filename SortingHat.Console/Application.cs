using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using System;

namespace SortingHat.CLI
{
    [UsedImplicitly]
    internal class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly ArgumentParser _argumentParser;
        private readonly ILoggerFactory _loggerFactory;

        public Application(ILogger<Application> logger, ILoggerFactory loggerFactory, ArgumentParser argumentParser)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _argumentParser = argumentParser;
        }

        internal void Run(string[] args)
        {
            try
            {
                _logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

                _argumentParser.Execute(args);
            }
            catch (ParseException e)
            {
                Console.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
                Console.WriteLine(e.Message);
                Environment.Exit(-1);

            }
            catch (SqliteException e)
            {
                _logger.LogWarning("The database is throwing an exception...");
                _logger.LogWarning(e.Message);
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("no such table:"))
                {
                    _logger.LogWarning("Database not initialized? Run .hat init");
                }

                _logger.LogError(e.Message);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.Exit(-1);
            }

            //Flush the logger
            _loggerFactory.Dispose();
        }
    }
}
