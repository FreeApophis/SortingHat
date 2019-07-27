namespace SortingHat.CLI.FileSystem
{
    class SystemFileMove: IMoveFile
    {
        public void Move(string sourceFileName, string destinationFileName)
        {
            System.IO.File.Move(sourceFileName,destinationFileName);
        }

        public void Move(string sourceFileName, string destinationFileName, bool overwrite)
        {
            System.IO.File.Move(sourceFileName, destinationFileName, overwrite);
        }
    }
}