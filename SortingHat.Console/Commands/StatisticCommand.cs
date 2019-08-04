using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using apophis.CLI;
using apophis.CLI.Writer;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class StatisticCommand : ICommand
    {
        private readonly IMainDatabase _db;
        private readonly IConsoleWriter _consoleWriter;

        public StatisticCommand(IMainDatabase db, IConsoleWriter consoleWriter)
        {
            _db = db;
            _consoleWriter = consoleWriter;
        }

        public string LongCommand => "statistics";
        public Option<string> ShortCommand => Option.Some("stat");
        public string ShortHelp => "Shows global statistics";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            var table = StatisticsTable();

            foreach (var (key, value) in _db.GetStatistics())
            {
                table.Append(key, "=", value);
            }

            _consoleWriter.WriteLine("Statistics:");
            _consoleWriter.WriteLine();
            table.WriteTo(_consoleWriter);

            return true;
        }

        private static ConsoleTable StatisticsTable()
        {
            var table = new ConsoleTable(3);

            table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;
            table.Columns[0].PaddingLeft = 2;

            return table;
        }
    }
}
