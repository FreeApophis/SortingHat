using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects;

[UsedImplicitly]
internal class ExportProjectCommand(DatabaseSettings databaseSettings, IConsoleWriter consoleWriter, ICopyFile copyFile) : IDeclaredCommand
{
    private const string CurrentDirectory = ".";

    private static readonly ArgumentDefinition ProjectNameArgument = new() { Name = "project-name", Description = "The project to export." };

    private static readonly ArgumentDefinition Destination = new() { Name = "destination", Description = "A folder, where the export is named '<project-name>.hdb', or a file. Default: the current folder.", Arity = Arity.ZeroOrOne };

    public CommandGrouping CommandGrouping => CommandGrouping.Project;
    public string Name => "export";
    public string Description => "Exports a project as a file.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [ProjectNameArgument, Destination];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(ProjectNameArgument).Match(
            none: false,
            some: project => ExportProject(project, input.Value(Destination).GetOrElse(CurrentDirectory))));

    private bool ExportProject(string project, string destination)
    {
        copyFile.Copy(DatabaseSource(project), DestinationPath(destination, project));
        consoleWriter.WriteLine($"Project '{project}' has been exported to: {DestinationPath(destination, project)}");

        return true;
    }

    private string DestinationPath(string destination, string project)
        => Directory.Exists(destination)
            ? Path.Combine(destination, $"{project}.hdb")
            : destination;

    private string DatabaseSource(string project)
        => Path.Combine(databaseSettings.DbPath, $"{project}.db");
}
