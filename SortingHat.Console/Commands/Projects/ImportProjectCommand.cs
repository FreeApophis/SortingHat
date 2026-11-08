using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
[method: UsedImplicitly]
internal class ImportProjectCommand(IConsoleWriter consoleWriter, DatabaseSettings databaseSettings, IProjects projects) : IDeclaredCommand
{
    private static readonly ArgumentDefinition ProjectFile = new() { Name = "project-file", Description = "A project file, for example made with export." };

    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The name of the new project. Default: the file name without its extension.", Arity = Arity.ZeroOrOne };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "import";
    public string Description => "Imports an exported project file as a new project.";
    public string Details => "Only a SortingHat project this hat can open is imported; a project of an older hat is updated when it is first used.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectFile, ProjectNameArgument];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectFile).Match(
            none: false,
            some: filePath => ImportProject(filePath, input.Value(ProjectNameArgument).GetOrElse(() => Path.GetFileNameWithoutExtension(filePath)))));

    private bool ImportProject(string filePath, string projectName)
        => (from project in ProjectName.ParseNew(projectName, projects.GetProjects(), databaseSettings.Name)
            from imported in projects.Import(filePath, project)
            select $"New project '{project}' imported.")
            .Match(left: Report, right: Imported);

    private bool Imported(string message)
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
