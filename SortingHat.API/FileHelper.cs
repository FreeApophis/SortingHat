using System.Diagnostics;

namespace SortingHat.API
{
    public class FileHelper
    {
        public static void OpenWithAssociatedProgram(string filePath)
        {
            var procStart = new ProcessStartInfo(filePath) { UseShellExecute = true, CreateNoWindow = true };

            //Creates a process
            var proc = new Process { StartInfo = procStart };
            //Start
            proc.Start();
        }
    }
}
