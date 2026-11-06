using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class ImportProjectCommand(IConsoleWriter consoleWriter, ICopyFile copyFile, DatabaseSettings databaseSettings, IProjects projects) : IDeclaredCommand
{
    private static readonly ArgumentDefinition ProjectFile = new() { Name = "project-file", Description = "A project file, for example made with export." };

    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The name of the new project. Default: the file name without its extension.", Arity = Arity.ZeroOrOne };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "import";
    public string Description => "Imports a project file into the db.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectFile, ProjectNameArgument];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectFile).Match(
            none: false,
            some: filePath => ImportProject(filePath, input.Value(ProjectNameArgument).GetOrElse(() => Path.GetFileNameWithoutExtension(filePath)))));

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

    private string DestinationPath(ProjectName project)
        => Path.Combine(databaseSettings.DbPath, $"{project.Value}.db");
}
