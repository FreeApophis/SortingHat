using System.Collections.Generic;

namespace SortingHat.ConsoleWriter
{
    public class MemoryConsoleWriter : IConsoleWriter
    {
        private List<string> _lines = new List<string>();

        public MemoryConsoleWriter()
        {
        }

        public IEnumerable<string> Lines => _lines;

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
