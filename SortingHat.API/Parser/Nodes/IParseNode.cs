namespace SortingHat.API.Parser.Nodes
{
    public interface IParseNode
    {
        void Accept(INodeVisitor visitor);
    }
}
