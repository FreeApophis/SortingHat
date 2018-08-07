using System;

namespace SortingHat.API.Parser.Nodes
{
    public class AndOperatorNode : BinaryOperatorNode
    {
        internal AndOperatorNode(IParseNode leftOperand, IParseNode rightOperand) :
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
                    return "∧";
                case OperatorType.Text:
                    return "and";
                case OperatorType.Programming:
                    return "&&";
            }

            throw new NotImplementedException();
        }
    }
}