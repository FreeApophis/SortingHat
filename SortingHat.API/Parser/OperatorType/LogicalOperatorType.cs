namespace SortingHat.API.Parser {
    public class LogicalOperatorType : IOperatorType
    {
        public string Not => "¬";
        public string NotEnd => "";
        public string And => "∧";
        public string Or => "∨";
    }
}