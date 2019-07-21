using System;

namespace SortingHat.ConsoleWriter
{
    public class SystemConsoleWriter : IConsoleWriter
    {
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}