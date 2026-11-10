using System;
using System.Collections.Generic;
using Autofac.Features.OwnedInstances;
using JetBrains.Annotations;
using Serilog;
using SortingHat.App;
using SortingHat.Desktop.Core;

namespace SortingHat.Desktop.Shell;

/// <summary>
/// Runs an <see cref="Effect"/> and returns what happened as messages. Every effect gets its own lifetime scope
/// (<see cref="Owned{T}"/>), like one run of hat, so it gets its own database connection.
/// </summary>
[UsedImplicitly]
internal sealed class EffectRunner(ILogger logger, Func<Owned<StartupReport>> startupReport)
{
    /// <summary>An effect that fails is a bug or a broken database: the user gets one line, the log the exception.</summary>
    public IReadOnlyList<Message> Run(Effect effect)
    {
        try
        {
            return effect switch
            {
                Effect.LoadStartupWarnings => LoadStartupWarnings(),
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "Every effect is handled."),
            };
        }
        catch (Exception exception)
        {
            logger.Error(exception, "{Effect} failed", effect);

            return [new Message.Wrote($"{effect.GetType().Name} failed: {exception.Message}")];
        }
    }

    private IReadOnlyList<Message> LoadStartupWarnings()
    {
        using var report = startupReport();

        return [new Message.StartupWarningsLoaded(report.Value.Warnings())];
    }
}
