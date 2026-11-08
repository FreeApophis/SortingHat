using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
internal class ExportProjectCommand(IProjects projects, DatabaseSettings databaseSettings, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private const string CurrentDirectory = ".";

    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The project to export." };

    private static readonly ArgumentDefinition Destination = new() { Name = "destination", Description = "A folder, where the export is named '<project-name>.hdb', or a new file. Default: the current folder.", Arity = Arity.ZeroOrOne };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "export";
    public string Description => "Exports a project as a file.";
    public string Details => "The export is a consistent copy, also of the active project. An existing file is never overwritten.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectNameArgument, Destination];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectNameArgument).Match(
            none: false,
            some: name => ExportProject(name, input.Value(Destination).GetOrElse(CurrentDirectory))));

    private static string DestinationPath(string destination, ProjectName project)
        => Directory.Exists(destination)
            ? Path.Combine(destination, $"{project}.hdb")
            : destination;

    /// <summary>The project as it was created, also when <paramref name="name"/> differs in case.</summary>
    private static Either<string, ProjectName> Existing(string name, IEnumerable<string> existingProjects, string mainDatabaseName)
        => existingProjects
            .FirstOrNone(project => ProjectName.IsSameName(name, project))
            .Match(
                none: () => Either<string, string>.Left($"There is no project with the name '{name}'."),
                some: Either<string, string>.Right)
            .SelectMany(project => ProjectName.Parse(project, mainDatabaseName));

    private bool ExportProject(string name, string destination)
        => (from project in Existing(name, projects.GetProjects(), databaseSettings.Name)
            let path = DestinationPath(destination, project)
            from exported in projects.Export(project, path)
            select $"Project '{project}' exported to: {path}")
            .Match(left: Report, right: Exported);

    private bool Exported(string message)
    {
        consoleWriter.WriteLine(message);

        return true;
    }

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
