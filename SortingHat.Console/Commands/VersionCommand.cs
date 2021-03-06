﻿using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using apophis.CLI;
using apophis.CLI.Writer;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class VersionCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;

        [UsedImplicitly]
        public VersionCommand(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        public string LongCommand => "version";
        public Option<string> ShortCommand => Option.Some("v");
        public string ShortHelp => "Shows the current Version of this program";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            var version = GetVersion();

            _consoleWriter.WriteLine($"{nameof(SortingHat)} {version.Version}");
            _consoleWriter.WriteLine();

            var table = new ConsoleTable(2);

            table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;

            table.Append("Culture:", version.CultureName ?? "<NULL>");
            table.Append("Current Culture:", CultureInfo.CurrentCulture);

            table.WriteTo(_consoleWriter);
            return true;
        }

        private static AssemblyName GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName();
        }

    }
}
