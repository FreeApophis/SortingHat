namespace SortingHat.CliAbstractions
{
    public class NullWriter : IConsoleWriter
    {
        public void WriteLine(string line)
        {
        }

        public void WriteLine()
        {
        }
    }
}
