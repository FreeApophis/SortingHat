using SortingHat.API.Parser.OperatorType;

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

        internal object ToString(IOperatorType operatorType)
        {
            return operatorType.And;
        }
    }
}