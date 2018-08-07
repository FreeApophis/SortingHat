using System;

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

        internal string ToString(OperatorType operatorType = OperatorType.Text)
        {
            switch (operatorType)
            {
                case OperatorType.Logical:
                    return "¬";
                case OperatorType.Text:
                    return "not";
                case OperatorType.Programming:
                    return "!";
            }

            throw new NotImplementedException();
        }
    }
}
