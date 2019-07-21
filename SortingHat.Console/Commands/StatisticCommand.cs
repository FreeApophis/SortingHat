using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System;
using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class StatisticCommand : ICommand
    {
        private readonly IDatabase _db;

        public StatisticCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var table = StatisticsTable();

            foreach (var (key, value) in _db.GetStatistics())
            {
                table.Append(key, "=", value);
            }

            Console.WriteLine("Statistics:");
            Console.WriteLine();
            Console.WriteLine(table.ToString());

            return true;
        }

        private static ConsoleTable StatisticsTable()
        {
            var table = new ConsoleTable(3);

            table.Columns[0].Alignment = ConsoleTableColumnAlignment.Right;
            table.Columns[0].PaddingLeft = 2;

            return table;
        }

        public string LongCommand => "statistics";
        public Option<string> ShortCommand => Option.Some("stat");
        public string ShortHelp => "Shows global statistics";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
