using System;
using System.Text;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.OperatorType;

namespace SortingHat.API.Parser;

internal class ToStringVisitor(IOperatorType operatorType) : INodeVisitor
{
    private readonly StringBuilder _resultBuilder = new();

    public string Result => _resultBuilder.ToString();

    public void Visit(UnaryOperatorNode op)
    {
        throw new NotSupportedException();
    }

    public void Visit(BinaryOperatorNode op)
    {
        throw new NotSupportedException();
    }

    public void Visit(NotOperatorNode op)
    {
        _resultBuilder.Append(op.ToString(operatorType));
        _resultBuilder.Append(operatorType.NotOpen);
        op.Operand.Accept(this);
        _resultBuilder.Append(operatorType.NotClose);
    }

    public void Visit(AndOperatorNode op)
    {
        _resultBuilder.Append("(");
        op.LeftOperand.Accept(this);
        _resultBuilder.Append($" {op.ToString(operatorType)} ");
        op.RightOperand.Accept(this);
        _resultBuilder.Append(")");
    }

    public void Visit(OrOperatorNode op)
    {
        _resultBuilder.Append("(");
        op.LeftOperand.Accept(this);
        _resultBuilder.Append($" {op.ToString(operatorType)} ");
        op.RightOperand.Accept(this);
        _resultBuilder.Append(")");
    }

    public void Visit(TagNode tag)
    {
        _resultBuilder.Append(tag);
    }

    public void Visit(BooleanNode boolean)
    {
        _resultBuilder.Append(boolean);
    }
}
