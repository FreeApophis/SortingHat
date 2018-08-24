using System;
using SortingHat.API.Parser.OperatorType;

namespace SortingHat.API.Parser.Nodes
{
    public class OrOperatorNode : BinaryOperatorNode
    {
        internal OrOperatorNode(IParseNode leftOperand, IParseNode rightOperand) :
            base(leftOperand, rightOperand)
        {
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal object ToString(IOperatorType operatorType)
        {
            return operatorType.Or;
        }
    }
}