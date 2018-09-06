namespace SortingHat.Plugin.Exif
{
    interface IMetaDataReader
    {
        string ReadString(string fileName, int directoryEntryID);
    }
}