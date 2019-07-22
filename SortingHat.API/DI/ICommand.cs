using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.API.DI
{
    public interface ICommand
    {
        CommandGrouping CommandGrouping { get; }

        string LongCommand { get; }

        /// <summary>
        /// Can be null
        /// </summary>
        Option<string> ShortCommand { get; }

        /// <summary>
        /// Should return a single line description
        /// </summary>
        /// <returns></returns>
        string ShortHelp { get; }

        bool Execute(IEnumerable<string> arguments, IOptions options);
    }
}
