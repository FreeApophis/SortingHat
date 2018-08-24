using JetBrains.Annotations;

namespace SortingHat.API
{
    [UsedImplicitly]
    public class DatabaseSettings
    {
        public string DBType { get; set; } = "";
        public string DBName { get; set; } = "";
        public string DBPath { get; set; } = "";
    }
}
