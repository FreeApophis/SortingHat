using System;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using JetBrains.Annotations;
using Serilog;
using SortingHat.API;
using SortingHat.App;

namespace SortingHat.CLI;

[UsedImplicitly]
internal class Application(ILogger logger, IConsoleWriter consoleWriter, CommandLine commandLine, CommandNames commandNames, StartupReport startupReport, DatabaseSettings databaseSettings)
{
    /// <summary>Runs hat with the command line <paramref name="args"/> and returns its exit code.</summary>
    internal int Run(string[] args)
    {
        startupReport.Warnings().ForEach(consoleWriter.WriteLine);
        commandNames.Warnings.ForEach(consoleWriter.WriteLine);

        try
        {
            logger.Debug("Running hat {Arguments}", args);

            return commandLine.Run(args);
        }

        // a bug, in hat or a plugin, or a broken database: the console gets one line, the log the whole exception
        catch (Exception exception)
        {
            logger.Error(exception, "hat {Arguments} failed", args);
            consoleWriter.WriteLine($"hat failed: {exception.Message} The details are in the log in '{HatLog.Folder(databaseSettings)}'.");

            return ExitCode.Failure;
        }
    }
}
