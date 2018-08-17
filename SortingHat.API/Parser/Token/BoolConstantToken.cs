namespace SortingHat.API.Parser.Token
{
    public class BoolConstantToken : IToken
    {
        public BoolConstantToken(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override string ToString() => $"Bool Value: {Value}";
    }
}
