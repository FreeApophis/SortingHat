using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;

//Console.WriteLine("SortingHat [command]");
//Console.WriteLine("<tag> = :tag:subtag:...");
//Console.WriteLine("    tags list <tag>");
//Console.WriteLine("    tags add <tag>");
//Console.WriteLine("    tags remove <tag>");
//Console.WriteLine("    files tag <tags> <files>");
//Console.WriteLine("    files untag <tags> <files>");
//Console.WriteLine("    files search <tag> [and|or [not] <tag>]");
//Console.WriteLine("    init [location]");
//Console.WriteLine("    repair");
//Console.WriteLine("    sort");

namespace SortingHat.CLI.Commands
{
    class HelpCommand : ICommand
    {
        private const string Command = "help";
        private readonly IServices _services;

        public HelpCommand(IServices services)
        {
            _services = services;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("XXX");
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
