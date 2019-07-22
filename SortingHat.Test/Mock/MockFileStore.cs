using System.Collections.Generic;
using System.Threading.Tasks;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test.Mock
{
    public class MockFileStore : IFile
    {
        private MockFileStore()
        {

        }

        public bool LoadByPath(File file)
        {
            throw new System.NotImplementedException();
        }

        public Task Tag(File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public Task Untag(File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Tag>> GetTags(File file)
        {
            throw new System.NotImplementedException();
        }

        internal static MockFileStore Create()
        {
            return new MockFileStore();
        }

        public IEnumerable<string> GetPaths()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetPaths(File file)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetNames(File file)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<File> Search(string query)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<File> GetDuplicates()
        {
            throw new System.NotImplementedException();
        }
    }
}