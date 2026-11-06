namespace Apophis.FileSystem;

public interface ICopyFile : IFileOperation
{
    void Copy(string sourceFileName, string destinationFileName);

    void Copy(string sourceFileName, string destinationFileName, bool overwrite);
}
