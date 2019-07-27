using System;
using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Projects
{
    [UsedImplicitly]
    internal class ExportProjectCommand : ICommand
    {
        public CommandGrouping CommandGrouping => CommandGrouping.Project;
        public string LongCommand => "export";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Exports a project as a file.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            throw new NotImplementedException();
        }
    }
}