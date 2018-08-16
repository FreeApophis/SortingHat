namespace SortingHat.API.Parser.Nodes
{
    public sealed class TagNode : IParseNode
    {
        internal TagNode(string tag)
        {
            Tag = tag;
        }
        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Tag { get; }

        public override string ToString()
        {
            return Tag;
        }
    }
}