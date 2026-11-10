using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Serilog;
using SortingHat.API.Plugin;

namespace SortingHat.App;

/// <summary>What a host tells the user when it starts: the plugins it left out and a log level it does not know.</summary>
[UsedImplicitly]
public sealed class StartupReport(ILogger logger, IPluginLoader pluginLoader, LogSettings logSettings)
{
    /// <summary>A line per problem for the user; the plugin problems are logged as well.</summary>
    public IReadOnlyList<string> Warnings()
    {
        pluginLoader.Problems.ForEach(problem => logger.Warning("Plugin {Plugin} not loaded: {Reason}", problem.Source, problem.Reason));

        return
        [
            .. pluginLoader.Problems.Select(problem => $"Warning: plugin '{problem.Source}' not loaded: {problem.Reason}."),
            .. logSettings.Level.LeftOrNone().ToEnumerable(),
        ];
    }
}
