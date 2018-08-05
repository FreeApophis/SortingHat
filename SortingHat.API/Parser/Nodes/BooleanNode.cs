namespace SortingHat.API.Parser.Nodes
{
    public class BooleanNode : IParseNode
    {
        internal BooleanNode(bool boolConstant)
        {
            BoolConstant = boolConstant;
        }
        public virtual void Accept(INodeVisitor visitor)
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
