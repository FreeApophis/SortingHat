namespace SortingHat.API.DI
{
    public interface IOptions
    {
        bool HasOption(string? shortOption, string? longOption);
    }
}
