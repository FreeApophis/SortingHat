using System;
using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class DestroyProjectCommand : ICommand
    {
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "remove-project";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Removes a project with all tags and files.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
