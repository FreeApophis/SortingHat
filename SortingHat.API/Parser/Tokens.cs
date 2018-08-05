using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.Parser
{
    public abstract class Token
    {
    }

    public abstract class OperatorToken : Token
    {
    }
    public class AndToken : OperatorToken
    {
        public override string ToString() => "Addition Operator";
    }

    public class OrToken : OperatorToken
    {
        public override string ToString() => "Subtraction Operator";
    }

    public class NotToken : OperatorToken
    {
        public override string ToString() => "Multiplication Operator";
    }

    public abstract class ParenthesisToken : Token
    {

    }

    public class OpenParenthesisToken : ParenthesisToken
    {
        public override string ToString() => "Opening Parenthesis";
    }

    public class ClosedParenthesisToken : ParenthesisToken
    {
        public override string ToString() => "Closing Parenthesis";
    }

    public class TagToken : Token
    {
        public TagToken(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public override string ToString() => $"Tag: {Value}";
    }

    public class BoolConstantToken : Token
    {
        public BoolConstantToken(bool value)
        {
            Value = value;
        }

        public bool Value { get; private set; }

        public override string ToString() => $"Bool Value: {Value}";
    }
}
