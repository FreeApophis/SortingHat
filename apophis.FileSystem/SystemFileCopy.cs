namespace apophis.FileSystem
{
    public class SystemFileCopy : ICopyFile
    {
        public void Copy(string sourceFileName, string destinationFileName)
        {
            System.IO.File.Copy(sourceFileName, destinationFileName);
        }

        public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
        {
            System.IO.File.Copy(sourceFileName, destinationFileName, overwrite);
        }

    }
}