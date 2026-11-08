using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>
/// The tables of hat: column headers and one rule under them, no boxes, so they stay readable when a script reads the output.
/// Every cell is plain text, never markup: a '[' in a path stays.
/// </summary>
internal static class LightTable
{
    public static Table Create(params IEnumerable<TableColumn> columns)
        => new Table().Border(TableBorder.Simple).AddColumns([.. columns]);

    public static TableColumn Column(string header)
        => new(new Text(header, new Style(decoration: Decoration.Bold)));

    public static Table AddCells(this Table table, params IEnumerable<string> cells)
        => table.AddRow(cells.Select(cell => new Text(cell)));
}
