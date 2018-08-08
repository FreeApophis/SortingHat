using SortingHat.API.FileTypeDetection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    internal class IdentifyCommand : ICommand
    {
        private const string Command = "identify";
        private const string CommandShort = "id";

        public IdentifyCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var detector = new FileTypeDetector();

            foreach (var argument in arguments.Skip(1))
            {
                Console.WriteLine($"File: {argument}");
                var fileType = detector.Identify(argument);

                if (fileType != null)
                {
                    Console.WriteLine($"C: {fileType.Category}");
                    Console.WriteLine($"N: {fileType.Name}");
                    Console.WriteLine($"E: {string.Join(",", fileType.Extensions)}");
                } else
                {
                    Console.WriteLine($"Unknown filetype");
                }
            }
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && (arguments.First() == Command || arguments.First() == CommandShort);
        }
    }
}
