namespace SortingHat.API
{
    public static class StringExtensions
    {
        public static bool IsTag(this string value)
        {
            return value.StartsWith(":");
        }
    }
}
