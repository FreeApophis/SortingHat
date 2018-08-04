﻿namespace SortingHat.API.Parser
{
    public class UnaryMinus : UnaryOperator
    {
        internal UnaryMinus(IParseNode operand) : base(operand)
        {
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "-";
        }
    }
}
