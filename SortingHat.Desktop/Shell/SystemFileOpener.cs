using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.Desktop.Shell;

/// <summary>Opens a file like a double-click in the file manager of the system.</summary>
[UsedImplicitly]
internal sealed class SystemFileOpener : IFileOpener
{
    public Option<string> Open(string path)
    {
        if (!File.Exists(path))
        {
            return $"'{path}' does not exist any more; 'hat repair' removes missing paths.";
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

            return Option<string>.None;
        }
        catch (Win32Exception exception)
        {
            return $"'{path}' could not be opened: {exception.Message}";
        }
    }
}
