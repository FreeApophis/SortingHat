using System;
using System.Collections.Generic;

namespace apophis.CLI.Writer
{
    public class MemoryConsoleWriter : IConsoleWriter
    {
        private List<string> _lines = new List<string>();

        public MemoryConsoleWriter()
        {
        }

        public List<string> Lines => _lines;

        public void WriteLine(string line)
        {
            _lines.Add(line);
        }

        public void WriteLine()
        {
            _lines.Add(string.Empty);
        }
    }
}
