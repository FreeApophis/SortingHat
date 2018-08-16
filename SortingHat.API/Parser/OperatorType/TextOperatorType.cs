namespace SortingHat.API.Parser {
    public class TextOperatorType : IOperatorType
    {

        public string Not => "not(";
        public string NotEnd => ")";
        public string And => "and";
        public string Or => "or";
    }
}