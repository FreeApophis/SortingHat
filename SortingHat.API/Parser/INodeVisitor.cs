using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{
    public interface INodeVisitor
    {
        void Visit(IParseNode op);

        void Visit(UnaryOperatorNode op);
        void Visit(BinaryOperatorNode op);

        void Visit(NotOperatorNode op);
        void Visit(AndOperatorNode op);
        void Visit(OrOperatorNode op);

        void Visit(TagNode number);
        void Visit(BooleanNode boolean);
    }
}