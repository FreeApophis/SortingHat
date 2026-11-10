using System.IO;
using Serilog;
using Serilog.Events;
using SortingHat.API;

namespace SortingHat.App;

/// <summary>hat's log: a file per day in the folder 'logs' next to the databases, the last 31 days kept.</summary>
public static class HatLog
{
    private const string Template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static string Folder(DatabaseSettings databaseSettings)
        => Path.Combine(databaseSettings.DbPath, "logs");

    /// <summary>Several hat processes may write to the same file at once, so the file is shared.</summary>
    public static Serilog.Core.Logger Create(DatabaseSettings databaseSettings, LogSettings logSettings)
        => new LoggerConfiguration()
            .MinimumLevel.Is(logSettings.Level.GetOrElse(LogEventLevel.Information))
            .WriteTo.File(Path.Combine(Folder(databaseSettings), "hat-.log"), outputTemplate: Template, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31, shared: true)
            .CreateLogger();
}
