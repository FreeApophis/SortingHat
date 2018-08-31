namespace SortingHat.CLI.Output
{
    static class NumberFormatter
    {
        private static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static string HumanSize(long size, int magnitude)
        {
            return size < 1000
                ? $"{size,4} {SizeSuffixes[magnitude]}"
                : HumanSize(size / 1000, magnitude + 1);
        }
        public static string FixedHumanSize(this long size)
        {
            return HumanSize(size, 0);
        }

        public static string ShortHash(this string hash)
        {
            return hash.Split(':')[1].Substring(0, 8);
        }
    }
}
