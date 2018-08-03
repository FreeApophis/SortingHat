using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SortingHat.API.DI;

namespace SortingHat.Test
{
    internal class MockLog
    {
        public MockLog(LogCategory logCategory, string message, string file, string caller, int line)
        {
            LogCategory = logCategory;
            Message = message;
            File = file;
            Caller = caller;
            Line = line;
        }

        public LogCategory LogCategory;
        public readonly string Message;
        public readonly string Caller;
        public readonly string File;
        public readonly int Line;
    }

    internal class MockLogger : ILogger
    {
        public List<MockLog> Lines = new List<MockLog>();

        public void Log(string message, [CallerFilePath]string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0)
        {
            Lines.Add(new MockLog(LogCategory.Info, message, file, caller, line));
        }

        public void Log(LogCategory logCategory, string message, [CallerFilePath]string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0)
        {
            Lines.Add(new MockLog(logCategory, message, file, caller, line));
        }
    }
}