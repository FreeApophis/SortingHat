using Funcky.Monads;

namespace SortingHat.API.DI
{
    /// <summary>
    /// Command line option representing long options (--long-option) and short options (-s)
    /// </summary>
    public interface IOption
    {
        Option<string> ShortOption { get; }
        Option<string> LongOption { get; }
        string ShortHelp { get; }
    }
}
