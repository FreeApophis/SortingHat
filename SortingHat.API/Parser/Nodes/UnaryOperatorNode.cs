namespace SortingHat.API.Parser.Nodes;

public class UnaryOperatorNode : IParseNode
{
    internal UnaryOperatorNode(IParseNode operand)
    {
        Operand = operand;
    }

    public IParseNode Operand { get; }

    public virtual void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}