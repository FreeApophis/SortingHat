using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI
{
    class Options : IOptions
    {
        readonly IEnumerable<string> _options;
        public Options(IEnumerable<string> options)
        {
            _options = options.Select(ToKeys);
        }

        public bool HasOption(string? shortOption, string? longOption)
        {
            return _options
                .Any(AnyOption(shortOption, longOption));

        }

        private string ToKeys(string option)
        {
            return option.TrimStart('-').ToLower();
        }

        private static Func<string, bool> AnyOption(string? shortOption, string? longOption)
        {
            return option => shortOption != null && option == shortOption.ToLower()
                || longOption != null && option == longOption.ToLower();
        }
    }
}
