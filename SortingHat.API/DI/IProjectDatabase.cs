namespace SortingHat.API.DI
{
    public interface IProjectDatabase : IDatabase
    {
        IFile File { get; }
        ITag Tag { get; }
    }
}
