using SortingHat.API.FileTypeDetection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    internal class IdentifyCommand : ICommand
    {

        public IdentifyCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var detector = new FileTypeDetector();

            foreach (var argument in arguments)
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
                    Console.WriteLine($"Unknown filetype");
                }
            }
            return true;
        }

        public string LongCommand => "identify";
        public string ShortCommand => "id";

        public string ShortHelp => "";
    }
}
