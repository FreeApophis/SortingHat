namespace SortingHat.API.Parser.OperatorType;

internal sealed class TextOperatorType : IOperatorType
{
    public string Not => "not";
    public string NotOpen => "(";
    public string NotClose => ")";
    public string And => "and";
    public string Or => "or";
}