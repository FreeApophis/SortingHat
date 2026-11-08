using Funcky.Extensions;
using Funcky.Monads;
using Serilog.Events;

namespace SortingHat.CLI;

/// <summary>The section 'Log' of hat-config.json; without it, hat logs Information and up.</summary>
internal sealed record LogSettings
{
    /// <summary>The lowest level written to the log: Verbose, Debug, Information, Warning, Error or Fatal.</summary>
    public string MinimumLevel { get; init; } = nameof(LogEventLevel.Information);

    /// <summary>The level, or why <see cref="MinimumLevel"/> is none; hat then logs Information and says so.</summary>
    public Either<string, LogEventLevel> Level
        => MinimumLevel.ParseEnumOrNone<LogEventLevel>().Match(
            none: () => Either<string, LogEventLevel>.Left($"Warning: '{MinimumLevel}' in the section 'Log' of hat-config.json is no log level, use Verbose, Debug, Information, Warning, Error or Fatal; hat logs Information and up."),
            some: Either<string, LogEventLevel>.Right);
}
