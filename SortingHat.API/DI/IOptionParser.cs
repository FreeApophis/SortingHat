namespace SortingHat.API.DI
{
    public interface IOptionParser
    {
        bool HasOption<TOption>() where TOption : IOption, new();
    }
}
