using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortingHat.CLI.Commands
{
    class FileMoveCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public bool Match(IEnumerable<string> arguments)
        {
            if (arguments.Count() > 2)
            {
                var matcher = new Regex("files?", RegexOptions.IgnoreCase);

                if (matcher.IsMatch(arguments.First()))
                {
                    return arguments.Skip(1).First() == "mv";
                }
            }

            return false;
        }

        public string ShortHelp => "";

    }
}
