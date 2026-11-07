namespace SortingHat.CLI;

/// <summary>Command line arguments that start with a colon are tags, all others are files or file patterns.</summary>
internal static class StringExtensions
{
    extension(string value)
    {
        public bool IsTag
            => value.StartsWith(":");

        public bool IsFile
            => !value.IsTag;
    }
}
