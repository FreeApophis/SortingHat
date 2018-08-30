namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        bool Execute();
    }
}
