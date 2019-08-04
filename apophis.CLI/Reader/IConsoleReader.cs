using Funcky.Monads;

namespace apophis.CLI.Reader
{
    public interface IConsoleReader
    {
        Option<int> ReadInt();
        string ReadLine();
    }
}