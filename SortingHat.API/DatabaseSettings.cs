using JetBrains.Annotations;

namespace SortingHat.API
{
    [UsedImplicitly]
    public class DatabaseSettings
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string DefaultProject { get; set; } = "";
    }
}
