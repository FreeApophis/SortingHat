using System;

namespace SortingHat.CliAbstractions
{
    public class SystemConsoleWriter : IConsoleWriter
    {
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}