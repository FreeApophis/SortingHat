namespace SortingHat.CliAbstractions.Formatting
{
    public static class NumberFormatter
    {
        private static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string HumanSize(this long size)
        {
            var magnitude = 0;

            while (size >= 1000)
            {
                size = size / 1000;
                magnitude = magnitude + 1;
            }

            return $"{size} {SizeSuffixes[magnitude]}";
        }

        public static string ShortHash(this string hash)
        {
            return hash.Split(':')[1].Substring(0, 8);
        }
    }
}
