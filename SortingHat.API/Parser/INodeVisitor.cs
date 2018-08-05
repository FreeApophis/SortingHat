using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{
    public interface INodeVisitor
    {
        void Visit(UnaryOperatorNode op);
        void Visit(BinaryOperatorNode op);

        void Visit(NotOperatorNode op);
        void Visit(AndOperatorNode op);
        void Visit(OrOperatorNode op);

        void Visit(TagNode tag);
        void Visit(BooleanNode boolean);
    }
}