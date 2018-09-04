using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Plugin.FileType
{
    class FileTypeTaggerCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            return false;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.AutoTagging;
        public string LongCommand => "ft-tag";
        public string ShortCommand => null;
        public string ShortHelp => "Tagging according to file type";
    }
}
