namespace SortingHat.API.Parser.Nodes
{
    public sealed class BooleanNode : IParseNode
    {
        internal BooleanNode(bool boolConstant)
        {
            BoolConstant = boolConstant;
        }
        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public bool BoolConstant { get; }

        public override string ToString()
        {
            return BoolConstant ? "true" : "false";
        }
    }
}
