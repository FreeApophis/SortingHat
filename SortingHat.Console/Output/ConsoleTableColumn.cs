namespace SortingHat.CLI.Output
{
    class ConsoleTableColumn
    {
        public ConsoleTableColumnAlignment Alignment { get; set; } = ConsoleTableColumnAlignment.Left;
        public int PaddingLeft { get; set; } = 0;
        public int PaddingRight { get; set; } = 1;
    }
}
