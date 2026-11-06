using System;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Parser;
using Console = Apophis.CLI.Console;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application(ILogger<Application> logger, Console console, CommandLine commandLine)
{
    /// <summary>Runs hat with the command line <paramref name="args"/> and returns its exit code.</summary>
    internal int Run(string[] args)
    {
        try
        {
            logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

            return commandLine.Run(args);
        }
        catch (ParseException e)
        {
            console.Writer.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
            console.Writer.WriteLine(e.Message);

            return ExitCode.Failure;
        }
        catch (SqliteException e)
        {
            logger.LogWarning("The database is throwing an exception...");
            logger.LogWarning(e.Message);
            console.Writer.WriteLine(e.Message);

            return ExitCode.Failure;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            console.Writer.WriteLine(e.Message);

            if (e.StackTrace != null)
            {
                console.Writer.WriteLine(e.StackTrace);
            }

            return ExitCode.Failure;
        }
    }
}
