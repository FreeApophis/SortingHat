using System.Diagnostics;

namespace SortingHat.CLI;

/// <summary>Opens files the way the operating system does.</summary>
internal static class FileHelper
{
    public static void OpenWithAssociatedProgram(string filePath)
    {
        var procStart = new ProcessStartInfo(filePath) { UseShellExecute = true, CreateNoWindow = true };

        // Creates a process
        using var proc = new Process { StartInfo = procStart };

        // Start
        proc.Start();
    }
}
