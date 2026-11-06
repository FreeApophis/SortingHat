using System.IO;
using System.Text;

namespace Apophis.CLI.Writer;

/// <summary>
/// A <see cref="TextWriter"/> for libraries that write to one, which passes every complete line on to the <see cref="IConsoleWriter"/>.
/// </summary>
public sealed class ConsoleTextWriter(IConsoleWriter consoleWriter) : TextWriter
{
    private readonly StringBuilder _line = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        switch (value)
        {
            case '\r':
                break;
            case '\n':
                consoleWriter.WriteLine(_line.ToString());
                _line.Clear();
                break;
            default:
                _line.Append(value);
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _line.Length > 0)
        {
            consoleWriter.WriteLine(_line.ToString());
            _line.Clear();
        }

        base.Dispose(disposing);
    }
}
