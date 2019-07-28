namespace apophis.FileSystem
{
    public class SystemFileExists: IExistsFile
    {
        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }
    }
}