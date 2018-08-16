namespace SortingHat.API.Parser {
    public class ProgrammingOperatorType : IOperatorType
    {
        public string Not => "!";
        public string NotEnd => "";
        public string And => "&&";
        public string Or => "||";
    }
}