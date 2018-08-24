using System;
using SortingHat.API.Parser.OperatorType;

namespace SortingHat.API.Parser.Nodes
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

        internal object ToString(IOperatorType operatorType)
        {
            return operatorType.Not;
        }

    }
}
