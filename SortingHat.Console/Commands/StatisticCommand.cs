using SortingHat.API.DI;
using SortingHat.CLI.Output;
using System;
using System.Collections.Generic;

namespace SortingHat.CLI.Commands
{
    class StatisticCommand : ICommand
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
            Console.WriteLine(table.ToString());

            return true;
        }

        private ConsoleTable StatisticsTable()
        {
            var table = new ConsoleTable();

            table.Columns.Add(new ConsoleTableColumn() { Alignment = ConsoleTableColumnAlignment.Right});
            table.Columns.Add(new ConsoleTableColumn());
            table.Columns.Add(new ConsoleTableColumn());

            return table;
        }

        public string LongCommand => "statistics";

        public string ShortCommand => "stat";

        public string ShortHelp => "Shows global statistics";

    }
}
