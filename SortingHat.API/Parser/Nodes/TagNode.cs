namespace SortingHat.API.Parser.Nodes
{
    public class TagNode : IParseNode
    {
        internal TagNode(string tag)
        {
            Tag = tag;
        }
        public virtual void Accept(INodeVisitor visitor)
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