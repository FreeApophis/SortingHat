﻿using static System.Linq.Enumerable;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System;

namespace SortingHat.CLI.Output
{
    internal class ConsoleTable
    {
        private const char Space = ' ';

        public List<ConsoleTableColumn> Columns { get; } = new List<ConsoleTableColumn>();
        private readonly List<object[]> _rows = new List<object[]>();


        public ConsoleTable(int columns = 0)
        {
            foreach (var _ in Range(0, columns))
            {
                Columns.Add(new ConsoleTableColumn());
            }
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
            return _rows.Max(row => ToTableString(row[columnIndex]).Length);
        }

        private static string ToTableString(object data)
        {
            return data == null
                ? string.Empty
                : data.ToString();
        }

        private static string AlignmentFormat(ConsoleTableColumnAlignment alignment)
        {
            return alignment == ConsoleTableColumnAlignment.Left ? "{{{0},-{1}}}" : "{{{0},{1}}}";
        }

        private object Spaces(int spaces)
        {
            return new string(Space, spaces);
        }

        private string FormatCell(ConsoleTableColumn column)
        {
            return $"{Spaces(column.PaddingLeft)}{AlignmentFormat(column.Alignment)}{Spaces(column.PaddingRight)}";
        }

        private string GetFormat()
        {
            var stringBuilder = new StringBuilder();
            foreach (var (column, index) in Columns.WithIndex())
            {
                stringBuilder.Append(string.Format(FormatCell(column), index, MaxColumnLength(index)));
            }

            return stringBuilder.ToString();
        }



        public override string ToString()
        {
            return string.Join(Environment.NewLine, _rows.Select(row => string.Format(GetFormat(), row)));
        }
    }
}