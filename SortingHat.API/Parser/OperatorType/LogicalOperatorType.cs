namespace SortingHat.API.Parser.OperatorType;

public class LogicalOperatorType : IOperatorType
{
    public string Not => "¬";
    public string NotOpen => string.Empty;
    public string NotClose => string.Empty;
    public string And => "∧";
    public string Or => "∨";
}