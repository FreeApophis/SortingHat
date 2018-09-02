using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SortingHat.CLI.Output
{
    class ConsoleTable
    {
        public List<ConsoleTableColumn> Columns { get; } = new List<ConsoleTableColumn>();
        private List<object[]> _rows = new List<object[]>();


        public ConsoleTable()
        {
        }

        public void Append(params object[] args)
        {
            if (args.Length != Columns.Count)
            {
                throw new InvalidConstraintException();
            }

            _rows.Add(args);
        }

        private int MaxColumnLength(int columnIndex)
        {
            return _rows.Max(row => row[columnIndex].ToString().Length);
        }

        private string AlignmentFormat(ConsoleTableColumn column)
        {
            return column.Alignment == ConsoleTableColumnAlignment.Left ? "{{{0},-{1}}}" : "{{{0},{1}}}";
        }

        private string GetFormat()
        {
            var stringBuilder = new StringBuilder();
            foreach (var (column, index) in Columns.WithIndex())
            {
                stringBuilder.Append(string.Format(AlignmentFormat(column), index, MaxColumnLength(index)));
            }


            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _rows.Select(row => string.Format(GetFormat(), row)));
        }


    }


}
