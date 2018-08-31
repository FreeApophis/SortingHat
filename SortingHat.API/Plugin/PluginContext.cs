using System;

namespace SortingHat.API.Plugin
{
    [Serializable]
    public class PluginContext
    {
        public string FilePath { get; set; }

        public bool CanDeletePlugin { get; set; }
    }
}
