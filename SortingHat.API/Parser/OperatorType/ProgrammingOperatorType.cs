namespace SortingHat.API.Parser.OperatorType
{
    public class ProgrammingOperatorType : IOperatorType
    {
        public string Not => "!";
        public string NotOpen => "";
        public string NotClose => "";
        public string And => "&&";
        public string Or => "||";
    }
}