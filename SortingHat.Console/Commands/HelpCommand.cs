using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class HelpCommand : ICommand
    {
        private const string Command = "help";
        private readonly ILogger<HelpCommand> _logger;

        public HelpCommand(ILogger<HelpCommand> logger)
        {
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            _logger.Log(LogLevel.Information, "Execute Help Command");
            return true;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
