namespace SortingHat.CLI.Output
{
    class ConsoleTableColumn
    {
        public ConsoleTableColumnAlignment Alignment { get; set; } = ConsoleTableColumnAlignment.Left;
        public int SpaceLeft { get; set; } = 1;
        public int SpaceRight { get; set; } = 1;
    }
}
