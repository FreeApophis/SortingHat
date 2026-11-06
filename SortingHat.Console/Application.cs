using System;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using Console = Apophis.CLI.Console;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application(ILogger<Application> logger, Console console, ArgumentParser argumentParser)
{
    internal void Run(string[] args)
    {
        try
        {
            logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

            argumentParser.Execute(args);
        }
        catch (ParseException e)
        {
            console.Writer.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
            console.Writer.WriteLine(e.Message);
            Environment.Exit(-1);
        }
        catch (SqliteException e)
        {
            logger.LogWarning("The database is throwing an exception...");
            logger.LogWarning(e.Message);
            console.Writer.WriteLine(e.Message);
            Environment.Exit(-1);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            console.Writer.WriteLine(e.Message);

            if (e.StackTrace != null)
            {
                console.Writer.WriteLine(e.StackTrace);
            }

            Environment.Exit(-1);
        }
    }
}
