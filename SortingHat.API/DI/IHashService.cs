using System.Threading.Tasks;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

public interface IHashService
{
    Task<Hash> GetHash(string path);
}