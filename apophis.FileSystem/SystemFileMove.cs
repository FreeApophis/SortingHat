namespace apophis.FileSystem
{
    public class SystemFileMove: IMoveFile
    {
        public void Move(string sourceFileName, string destinationFileName)
        {
            System.IO.File.Move(sourceFileName,destinationFileName);
        }
    }
}