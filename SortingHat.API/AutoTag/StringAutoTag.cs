﻿using System.IO;
using System.Text.RegularExpressions;

namespace SortingHat.API.AutoTag
{
    public abstract class StringAutoTag : IAutoTag
    {
        public abstract string AutoTagKey { get; }
        public abstract string Description { get; }
        public string HumanReadableAutoTagsKey
            => AutoTagKey.Replace("<>", "<STRING> Any valid String of characters");

        public abstract string? HandleTag(FileInfo file, string? tagMatch);

        public string? FindMatch(string value)
        {
            var findString = new Regex(AutoTagKey.Replace("<>", @"(\w+)"));
            var match = findString.Match(value);

            return match.Success
                ? match.Groups[1].Value
                : null;
        }

    }
}
