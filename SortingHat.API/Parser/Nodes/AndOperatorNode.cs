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

        public override string ToString()
        {
            return "∧";
        }
    }
}