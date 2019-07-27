namespace SortingHat.CLI.FileSystem
{
    class SystemFileExists: IExistsFile
    {
        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }
    }
}