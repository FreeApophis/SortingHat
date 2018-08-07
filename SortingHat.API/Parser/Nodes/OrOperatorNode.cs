using System;

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

        internal object ToString(OperatorType operatorType = OperatorType.Text)
        {
            switch (operatorType)
            {
                case OperatorType.Logical:
                    return "∨";
                case OperatorType.Text:
                    return "or";
                case OperatorType.Programming:
                    return "||";
            }

            throw new NotImplementedException();
        }
    }
}