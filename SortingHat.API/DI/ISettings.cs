namespace SortingHat.API.DI
{
    public interface ISettings
    {
        string this[string key] { get; set; }
        bool HasValue(string key);
    }
}