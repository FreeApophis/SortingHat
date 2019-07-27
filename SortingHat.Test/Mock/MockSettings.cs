using SortingHat.API.DI;

namespace SortingHat.Test.Mock
{
    public class MockSettings : ISettings
    {
        public static MockSettings Create()
        {
            return new MockSettings();
        }

        public string this[string key]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public bool HasValue(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}