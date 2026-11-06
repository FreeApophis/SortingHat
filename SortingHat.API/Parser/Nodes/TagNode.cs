namespace SortingHat.API.Parser.Nodes;

public sealed class TagNode : IParseNode
{
    internal TagNode(string tag)
    {
        Tag = tag;
    }

    public string Tag { get; }

    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return Tag;
    }
}