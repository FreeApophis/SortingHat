namespace apophis.FileSystem
{
    public class SystemDirectoryExists : IExistsDirectory
    {
        public bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
    }
}