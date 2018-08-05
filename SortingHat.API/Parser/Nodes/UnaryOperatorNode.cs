namespace SortingHat.API.Parser.Nodes
{
    public class UnaryOperatorNode : IParseNode
    {
        public IParseNode Operand { get; set; }

        internal UnaryOperatorNode(IParseNode operand)
        {
            Operand = operand;

        }

        public virtual void Accept(INodeVisitor visitor)
        {
        }
    }
}