using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class ImportProjectCommand(IConsoleWriter consoleWriter, ICopyFile copyFile, DatabaseSettings databaseSettings, IProjects projects) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string LongCommand => "import";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Imports a project file into the db.";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        => arguments.ToList() switch
        {
            [] => NotEnoughArguments(),
            [var file] => ImportProject(file),
            [var file, var projectName] => ImportProject(file, projectName),
            _ => TooManyArguments(),
        };

    private bool TooManyArguments()
    {
        consoleWriter.WriteLine("Too many arguments given, please give only filePath and projectName.");

        return false;
    }

    private bool ImportProject(string filePath, string projectName)
        => ProjectName.ParseNew(projectName, projects.GetProjects(), databaseSettings.Name)
            .Match(left: Reject, right: project => Import(filePath, project));

    private bool Reject(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }

    private bool Import(string filePath, ProjectName projectName)
    {
        copyFile.Copy(filePath, DestinationPath(projectName));
        projects.AddProject(projectName);

        consoleWriter.WriteLine($"New project '{projectName}' imported.");

        return true;
    }

    private bool ImportProject(string filePath)
    {
        var projectName = Path.GetFileNameWithoutExtension(filePath);

        if (projectName == null)
        {
            throw new NullReferenceException(nameof(projectName));
        }

        return ImportProject(filePath, projectName);
    }

    private bool NotEnoughArguments()
    {
        throw new NotImplementedException();
    }

    private string DestinationPath(ProjectName project)
        => Path.Combine(databaseSettings.DbPath, $"{project.Value}.db");
}