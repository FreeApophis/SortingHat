using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using SortingHat.API.Plugin;
using System;

namespace SortingHat.CLI
{
    class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly ArgumentParser _argumentParser;
        private readonly IPluginLoader _pluginLoader;
        private readonly ILoggerFactory _loggerFactory;

        public Application(ILogger<Application> logger, ILoggerFactory loggerFactory, ArgumentParser argumentParser, IPluginLoader pluginLoader)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _argumentParser = argumentParser;
            _pluginLoader = pluginLoader;
        }

        internal void Run(string[] args)
        {
            try
            {
                _logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

                _pluginLoader.Load(AppContext.BaseDirectory);

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
