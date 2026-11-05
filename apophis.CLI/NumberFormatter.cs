using Funcky.Monads;

namespace apophis.CLI
{
    public static class NumberFormatter
    {
        private const int ShortHashLength = 8;

        private static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        extension(long size)
        {
            public string HumanSize()
                => HumanSize(size, magnitude: 0);
        }

        extension(string hash)
        {
            /// <summary>
            /// The first digits of a hash in the form "algorithm:digest", or none if the hash is malformed.
            /// </summary>
            public Option<string> ShortHash()
                => hash.Split(':') is [{ Length: > 0 }, { Length: >= ShortHashLength } digest]
                    ? digest[..ShortHashLength]
                    : Option<string>.None;
        }

        private static string HumanSize(long size, int magnitude)
            => size >= 1000
                ? HumanSize(size / 1000, magnitude + 1)
                : $"{size} {SizeSuffixes[magnitude]}";
    }
}
