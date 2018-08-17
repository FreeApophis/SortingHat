namespace SortingHat.API.Parser.Nodes
{
    public class UnaryOperatorNode : IParseNode
    {
        public IParseNode Operand { get; }

        internal UnaryOperatorNode(IParseNode operand)
        {
            Operand = operand;

        }

        public virtual void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}