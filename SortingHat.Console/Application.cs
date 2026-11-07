using System;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Plugin;
using Console = Apophis.CLI.Console;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application(ILogger<Application> logger, Console console, CommandLine commandLine, IPluginLoader pluginLoader)
{
    /// <summary>Runs hat with the command line <paramref name="args"/> and returns its exit code.</summary>
    internal int Run(string[] args)
    {
        foreach (var problem in pluginLoader.Problems)
        {
            console.Writer.WriteLine($"Warning: plugin '{problem.Source}' not loaded: {problem.Reason}.");
        }

        try
        {
            logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

            return commandLine.Run(args);
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
