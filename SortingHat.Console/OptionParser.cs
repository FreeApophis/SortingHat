using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.API.DI;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI;

internal sealed class OptionParser : IOptionParser
{
    private readonly IEnumerable<string> _options;

    public OptionParser(IEnumerable<string> options)
    {
        _options = options.Select(ToKeys);
    }

    public bool HasOption<TOption>()
        where TOption : IOption, new()
    {
        var option = new TOption();

        return _options
            .Any(AnyOption(option.ShortOption, option.LongOption));
    }

    private string ToKeys(string option)
    {
        return option.TrimStart('-').ToLower();
    }

    private static Func<string, bool> AnyOption(Option<string> shortOption, Option<string> longOption)
    {
        return option => shortOption.Match(none: false, some: o => string.Equals(o, option, StringComparison.OrdinalIgnoreCase))
            || longOption.Match(none: false, some: o => string.Equals(o, option, StringComparison.OrdinalIgnoreCase));
    }
}
