namespace SortingHat.DB
{
    internal static class DBString
    {
        public static string ToComparison(long? value)
        {
            return value.HasValue
                ? $"= {value}"
                : "IS NULL";
        }

        public static string ToSQL(long? value)
        {
            return value.HasValue
                ? value.ToString()
                : "NULL";
        }
    }
}
