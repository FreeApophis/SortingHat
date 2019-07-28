namespace apophis.FileSystem
{
    public interface ICopyFile
    {
        void Copy(string sourceFileName, string destinationFileName);
        void Copy(string sourceFileName, string destinationFileName, bool overwrite);
    }
}
