namespace SortingHat.CLI.FileSystem
{
    interface IMoveFile
    {
        void Move(string sourceFileName, string destinationFileName);
        void Move(string sourceFileName, string destinationFileName, bool overwrite);

    }
}
