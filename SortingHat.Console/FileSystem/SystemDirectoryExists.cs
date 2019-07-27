namespace SortingHat.CLI.FileSystem
{
    class SystemDirectoryExists : IExistsDirectory
    {
        public bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
    }
}