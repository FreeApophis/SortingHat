using System;
using System.IO;

namespace SortingHat.API.AutoTag
{
    public abstract class ConstantAutoTag : IAutoTag
    {
        public abstract string AutoTagKey { get; }
        public abstract string Description { get; }
        public string HumanReadableAutoTagsKey => AutoTagKey;

        public string HandleTag(FileInfo file, string tagMatch)
        {
            return HandleTag(file);
        }

        protected abstract string HandleTag(FileInfo file);

        public string FindMatch(string value)
        {
            return value == AutoTagKey ? value : null;
        }
    }
}
