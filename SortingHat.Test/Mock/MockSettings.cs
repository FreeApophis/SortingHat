using SortingHat.API.DI;

namespace SortingHat.Test.Mock;

public class MockSettings : ISettings
{
    public string? this[string key]
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }

    public static MockSettings Create()
    {
        return new MockSettings();
    }

    public bool HasValue(string key)
    {
        throw new System.NotImplementedException();
    }
}