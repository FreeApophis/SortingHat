namespace apophis.FileSystem
{
    public interface IMoveFile : IFileOperation
    {
        void Move(string sourceFileName, string destinationFileName);
    }
}
