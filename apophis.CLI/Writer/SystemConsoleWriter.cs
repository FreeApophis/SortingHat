using System;

namespace apophis.CLI.Writer
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