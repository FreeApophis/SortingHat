using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SortingHat.CLI.Commands
{
    class RenameTagCommand : ICommand
    {
        private const string Command = "tag rename";
        private readonly ILogger<RenameTagCommand> _logger;

        public RenameTagCommand(ILogger<RenameTagCommand> logger)
        {
            _logger = logger;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            throw new System.NotImplementedException();
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return false;
        }

                public string ShortHelp => "";

    }
}
