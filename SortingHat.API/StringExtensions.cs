namespace SortingHat.API
{
    public static class StringExtensions
    {
        extension(string value)
        {
            public bool IsTag()
                => value.StartsWith(":");

            public bool IsFile()
                => !value.IsTag();
        }
    }
}
