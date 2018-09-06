namespace SortingHat.Plugin.Exif.TagTransformer
{
    public interface ITagTransformer
    {
        string TransformTag(string fileName, int directoryEntryID);
    }
}
