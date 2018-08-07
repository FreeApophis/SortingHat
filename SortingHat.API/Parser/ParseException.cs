using System;

namespace SortingHat.API.Parser
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
