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

        public override string ToString()
        {
            return "∨";
        }
    }
}