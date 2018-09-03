using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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

        public bool Execute(IEnumerable<string> arguments)
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

        public string ShortCommand => "stat";

        public string ShortHelp => "Shows global statistics";

    }
}
