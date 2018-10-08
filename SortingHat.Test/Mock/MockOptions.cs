using SortingHat.API.DI;


namespace SortingHat.Test.Mock
{
    class MockOptions : IOptions
    {
        public bool HasOption(string shortOption, string longOption)
        {
            return false;
        }
    }
}
