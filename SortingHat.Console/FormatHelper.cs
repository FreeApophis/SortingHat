namespace SortingHat.CLI
{
    static class FormatHelper
    {
        private static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static string HumanSize(long size, int magnitude)
        {
            if (size < 1000)
            {
                return $"{size,4} {SizeSuffixes[magnitude]}";
            }

            return HumanSize(size / 1000, magnitude + 1);

        }
        public static string FixedHumanSize(this long size)
        {
            return HumanSize(size, 0);
        }
    }
}
