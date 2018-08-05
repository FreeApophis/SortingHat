﻿namespace SortingHat.API.Parser.Nodes
{
    public class NotOperatorNode : UnaryOperatorNode
    {
        internal NotOperatorNode(IParseNode operand) : base(operand)
        {
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "¬";
        }
    }
}