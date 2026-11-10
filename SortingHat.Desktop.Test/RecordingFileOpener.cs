using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.Desktop.Shell;

namespace SortingHat.Desktop.Test;

/// <summary>Opening a file starts another program, so the tests only note which paths the app wanted to open.</summary>
internal sealed class RecordingFileOpener : IFileOpener
{
    private readonly List<string> _opened = [];

    public IReadOnlyList<string> Opened => _opened;

    public Option<string> Open(string path)
    {
        _opened.Add(path);

        return Option<string>.None;
    }
}
