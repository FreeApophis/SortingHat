using System.Threading.Tasks;

namespace SortingHat.API.DI
{
    public interface IHashService
    {
        Task<string> GetHash(string path);
    }
}