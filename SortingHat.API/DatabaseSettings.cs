using System;
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

        public string DbPath => CalculatDbPath();

        private string CalculatDbPath()
        {
            var basePath = Path switch
            {
                "#USERDOC" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "#APPDATA" => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                _ => Path
            };

            return System.IO.Path.Combine(basePath, ".hat");
        }
    }
}
