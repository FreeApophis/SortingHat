namespace SortingHat.API.Parser.OperatorType;

internal interface IOperatorType
{
    string Not { get; }
    string NotOpen { get; }
    string NotClose { get; }
    string And { get; }
    string Or { get; }
}