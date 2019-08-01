using Funcky.Monads;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI
{
    class OptionParser : IOptionParser
    {
        readonly IEnumerable<string> _options;
        public OptionParser(IEnumerable<string> options)
        {
            _options = options.Select(ToKeys);
        }

        public bool HasOption(IOption option)
        {
            return _options
                .Any(AnyOption(option.ShortOption, option.LongOption));

        }

        private string ToKeys(string option)
        {
            return option.TrimStart('-').ToLower();
        }

        private static Func<string, bool> AnyOption(Option<string> shortOption, Option<string> longOption)
        {
            return option => shortOption.Match(false, o => string.Equals(o, option, StringComparison.OrdinalIgnoreCase))
                || longOption.Match(false, o => string.Equals(o, option, StringComparison.OrdinalIgnoreCase));
        }
    }
}
