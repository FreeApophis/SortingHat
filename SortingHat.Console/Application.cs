using System;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SortingHat.API.Plugin;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application(ILogger<Application> logger, IConsoleWriter consoleWriter, CommandLine commandLine, CommandNames commandNames, IPluginLoader pluginLoader)
{
    /// <summary>Runs hat with the command line <paramref name="args"/> and returns its exit code.</summary>
    internal int Run(string[] args)
    {
        foreach (var problem in pluginLoader.Problems)
        {
            consoleWriter.WriteLine($"Warning: plugin '{problem.Source}' not loaded: {problem.Reason}.");
        }

        commandNames.Warnings.ForEach(consoleWriter.WriteLine);

        try
        {
            logger.LogTrace($"Running application instance with args: {string.Join(" ", args)}");

            return commandLine.Run(args);
        }
        catch (SqliteException e)
        {
            logger.LogWarning("The database is throwing an exception...");
            logger.LogWarning(e.Message);
            consoleWriter.WriteLine(e.Message);

            return ExitCode.Failure;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            consoleWriter.WriteLine(e.Message);

            if (e.StackTrace != null)
            {
                consoleWriter.WriteLine(e.StackTrace);
            }

            return ExitCode.Failure;
        }
    }
}
