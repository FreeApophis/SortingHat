namespace apophis.CLI.Writer
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
