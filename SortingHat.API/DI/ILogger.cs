using System.Runtime.CompilerServices;

namespace SortingHat.API.Interfaces
{
    public interface ILogger
    {
        void Log(string message, [CallerFilePath]string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0);
        void Log(LogCategory logCategory, string message, [CallerFilePath]string file = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0);
    }
}
