using System;
using System.Runtime.CompilerServices;
using SortingHat.API.Interfaces;

namespace SortingHat.CLI
{
    internal class CLILogger : ILogger
    {
        public void Log(string message, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0)
        {
            Log(LogCategory.Info, message, file, caller, line);
        }

        public void Log(LogCategory logCategory, string message, [CallerFilePath] string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0)
        {
            Console.WriteLine($"{file,20} {line,5} {logCategory,10}: {message}");
        }
    }
}