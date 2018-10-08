namespace SortingHat.API
{
    public static class StringExtensions
    {
        public static bool IsTag(this string value)
        {
            return value.StartsWith(":");
        }

        public static bool IsFile(this string value)
        {
            return value.StartsWith(":") == false;
        }
    }
}
