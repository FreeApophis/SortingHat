namespace apophis.CLI
{
    public static class NumberFormatter
    {
        private static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        extension(long size)
        {
            public string HumanSize()
                => HumanSize(size, magnitude: 0);
        }

        extension(string hash)
        {
            public string ShortHash()
                => hash.Split(':')[1].Substring(0, 8);
        }

        private static string HumanSize(long size, int magnitude)
            => size >= 1000
                ? HumanSize(size / 1000, magnitude + 1)
                : $"{size} {SizeSuffixes[magnitude]}";
    }
}
