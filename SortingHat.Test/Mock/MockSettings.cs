using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockSettings : ISettings
    {
        public static MockSettings Create()
        {
            return new MockSettings();
        }
    }
}