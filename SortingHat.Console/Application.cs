using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using System;

namespace SortingHat.CLI
{
    class Application
    {
        ILogger<Application> _logger;
        readonly ArgumentParser _argumentParser;
        readonly ILoggerFactory _loggerFactory;

        public Application(ILogger<Application> logger, ArgumentParser argumentParser, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _argumentParser = argumentParser;
            _loggerFactory = loggerFactory;
        }

        internal void Run(string[] args)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"Running application instance with args: {string.Join(" ", args)}");

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
                Environment.Exit(-1);
            }

            //Flush the logger
            _loggerFactory.Dispose();
        }
    }
}
