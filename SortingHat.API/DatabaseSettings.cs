using System;
using JetBrains.Annotations;

namespace SortingHat.API;

[UsedImplicitly]
public class DatabaseSettings
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string DefaultProject { get; set; } = string.Empty;

    public string DbPath => CalculatDbPath();

    private string CalculatDbPath()
        => System.IO.Path.Combine(CalculateBasePath(), ".hat");

    private string CalculateBasePath()
        => Path switch
        {
            "#USERDOC" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "#APPDATA" => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _ => Path,
        };
}
