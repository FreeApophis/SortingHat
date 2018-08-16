using System;
using System.Collections.Generic;
using System.Reflection;

namespace SortingHat.CLI.Commands
{
    class VersionCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            var version = GetVersion();

            Console.WriteLine($"{version.FullName}");

            return true;
        }

        private static AssemblyName GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName();
        }

        public string LongCommand => "version";
        public string ShortCommand => "v";
        public string ShortHelp => "Shows the current Version of this program";
    }
}
