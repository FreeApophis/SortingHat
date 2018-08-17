using System;
using System.Text;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{

    public class ToStringVisitor : INodeVisitor
    {
        public ToStringVisitor(IOperatorType operatorType)
        {
            _operatorType = operatorType;
        }

        public string Result => _resultBuilder.ToString();
        private readonly StringBuilder _resultBuilder = new StringBuilder();
        private readonly IOperatorType _operatorType;

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
            _resultBuilder.Append(op.ToString(_operatorType));
            op.Operand.Accept(this);
            _resultBuilder.Append(_operatorType.NotEnd);
        }

        public void Visit(AndOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" {op.ToString(_operatorType)} ");
            op.RightOperand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(OrOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" {op.ToString(_operatorType)} ");
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
}
