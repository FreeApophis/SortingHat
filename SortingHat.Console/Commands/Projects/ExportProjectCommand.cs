using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
internal class ExportProjectCommand : ICommand
{
    private const string CurrentDirectory = ".";
    private readonly DatabaseSettings _databaseSettings;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICopyFile _copyFile;

    public ExportProjectCommand(DatabaseSettings databaseSettings, IConsoleWriter consoleWriter, ICopyFile copyFile)
    {
        _databaseSettings = databaseSettings;
        _consoleWriter = consoleWriter;
        _copyFile = copyFile;
    }

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "export";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Exports a project as a file.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        => arguments.ToList() switch
        {
            [] => NotEnoughArguments(),
            [var projectName] => ExportProject(projectName, CurrentDirectory),
            [var projectName, var path] => ExportProject(projectName, path),
            _ => TooManyArguments(),
        };

    private bool TooManyArguments()
    {
        _consoleWriter.WriteLine("Too many arguments given, please only export one project at a time.");

        return false;
    }

    private bool NotEnoughArguments()
    {
        _consoleWriter.WriteLine("Too few arguments given, please specify which project to export.");

        return false;
    }

    private bool ExportProject(string project, string destination)
    {
        _copyFile.Copy(DatabaseSource(project), DestinationPath(destination, project));
        _consoleWriter.WriteLine($"Project '{project}' has been exported to: {DestinationPath(destination, project)}");

        return true;
    }

    private string DestinationPath(string destination, string project)
    {
        return Directory.Exists(destination)
            ? Path.Combine(destination, $"{project}.hdb")
            : destination;
    }

    private string DatabaseSource(string project)
    {
        return Path.Combine(_databaseSettings.DbPath, $"{project}.db");
    }
}