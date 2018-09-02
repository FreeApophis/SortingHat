using System.ComponentModel;

namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        bool Register();
    }
}
