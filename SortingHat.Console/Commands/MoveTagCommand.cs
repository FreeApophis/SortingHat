using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class MoveTagCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public string LongCommand => "move-tag";
        public string ShortCommand => null;
        public string ShortHelp => "this moves a tag to another parent tag, if you want to move it to the root, use the empty tag ':'";
    }
}
