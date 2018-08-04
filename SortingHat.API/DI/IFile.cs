using SortingHat.API.Models;

namespace SortingHat.API.DI
{
    public interface IFile
    {
        void Tag(File file, Tag tag);
        void Untag(File file, Tag tag);

        void Search(string query);
    }
}
