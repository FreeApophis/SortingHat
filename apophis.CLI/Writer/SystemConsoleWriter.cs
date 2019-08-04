namespace apophis.CLI.Writer
{
    public class SystemConsoleWriter : IConsoleWriter
    {
        public void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }

        public void WriteLine()
        {
            System.Console.WriteLine();
        }
    }
}