﻿using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.Plugin.ExtractRelevant
{
    internal class ScanCommand : ICommand
    {
        private readonly FolderScanner _folderScanner;

        public ScanCommand(FolderScanner folderScanner)
        {
            _folderScanner = folderScanner;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.File;

        public string LongCommand => "scan";

        public string ShortCommand => "s";

        public string ShortHelp => "scans a folder and its content";

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            return _folderScanner.Scan(arguments);
        }
    }
}