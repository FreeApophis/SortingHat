﻿using SortingHat.API.FileTypeDetection;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class IdentifyCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            var detector = new FileTypeDetector();
            var filePathExtractor = new FilePathExtractor(arguments);

            foreach (var argument in filePathExtractor.FilePaths)
            {
                Console.WriteLine($"File: {argument}");
                var fileType = detector.Identify(argument);

                if (fileType != null)
                {
                    Console.WriteLine($"C: {fileType.Category}");
                    Console.WriteLine($"N: {fileType.Name}");
                    Console.WriteLine($"E: {string.Join(",", fileType.Extensions)}");
                }
                else
                {
                    Console.WriteLine("Unknown filetype");
                }
            }
            return true;
        }

        public string LongCommand => "identify";
        public string ShortCommand => "id";

        public string ShortHelp => "This command identifies the real file type (ignoring the file-extension)";
    }
}
