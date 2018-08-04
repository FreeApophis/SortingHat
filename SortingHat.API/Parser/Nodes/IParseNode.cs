namespace SortingHat.API.Parser
{
    public interface IParseNode
    {
        void Accept(INodeVisitor visitor);
    }
}
