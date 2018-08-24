namespace SortingHat.API.Models
{
    public interface ITagParser
    {
        Tag Parse(string tagString);
    }
}