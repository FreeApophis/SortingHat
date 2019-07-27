namespace SortingHat.CLI.FileSystem
{
    interface ICopyFile
    {
        void Copy(string sourceFileName, string destinationFileName);
        void Copy(string sourceFileName, string destinationFileName, bool overwrite);
    }
}
