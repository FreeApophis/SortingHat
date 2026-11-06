using System;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using Console = Apophis.CLI.Console;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application
{
    private readonly ILogger<Application> _logger;
    private readonly ArgumentParser _argumentParser;
    private readonly Console _console;

    public Application(ILogger<Application> logger, Console console, ArgumentParser argumentParser)
    {
        _logger = logger;
        _console = console;
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
            _console.Writer.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
            _console.Writer.WriteLine(e.Message);
            Environment.Exit(-1);
        }
        catch (SqliteException e)
        {
            _logger.LogWarning("The database is throwing an exception...");
            _logger.LogWarning(e.Message);
            _console.Writer.WriteLine(e.Message);
            Environment.Exit(-1);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _console.Writer.WriteLine(e.Message);

            if (e.StackTrace != null)
            {
                _console.Writer.WriteLine(e.StackTrace);
            }

            Environment.Exit(-1);
        }
    }
}
