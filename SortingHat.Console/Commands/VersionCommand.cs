using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class VersionCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var version = GetVersion();

            Console.WriteLine($"{nameof(SortingHat)} {version.Version}");
            Console.WriteLine();

            var table = new ConsoleTable(2);

            table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;

            table.Append("Culture:", version.CultureName);
            table.Append("Current Culture:", CultureInfo.CurrentCulture);

            Console.WriteLine(table.ToString());
            return true;
        }

        private static AssemblyName GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName();
        }

        public string LongCommand => "version";
        public string ShortCommand => "v";
        public string ShortHelp => "Shows the current Version of this program";
        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
