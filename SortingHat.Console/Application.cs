using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using System;

namespace SortingHat.CLI
{
    class Application
    {
        ILogger<Application> _logger;
        ArgumentParser _argumentParser;

        public Application(ILogger<Application> logger, ArgumentParser argumentParser)
        {
            _logger = logger;
            _argumentParser = argumentParser;
        }

        internal void Run(string[] args)
        {
            try
            {
                _logger.Log(LogLevel.Trace, $"Running application instance with args: {args.ToString()}");

                _argumentParser.Execute(args);
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

    }
}
