namespace SortingHat.API.Parser {
    public interface IOperatorType
    {
        string Not { get; }
        string NotEnd { get; }
        string And { get; }
        string Or { get; }
    }
}