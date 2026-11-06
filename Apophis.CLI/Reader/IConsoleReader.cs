using Funcky.Monads;

namespace Apophis.CLI.Reader;

public interface IConsoleReader
{
    Option<int> ReadInt();

    string ReadLine();
}