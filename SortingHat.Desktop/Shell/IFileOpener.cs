using Funcky.Monads;

namespace SortingHat.Desktop.Shell;

/// <summary>Opens a file with the program the system has for it.</summary>
internal interface IFileOpener
{
    /// <returns>Why the file could not be opened, none when it was.</returns>
    Option<string> Open(string path);
}
