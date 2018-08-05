using System;
using System.Collections.Generic;
using System.Text;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{
    public class ToStringVisitor : INodeVisitor
    {
        public string Result => _resultBuilder.ToString();
        private StringBuilder _resultBuilder = new StringBuilder();

        public void Visit(UnaryOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(BinaryOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotOperatorNode op)
        {
            _resultBuilder.Append(op.ToString());
            op.Operand.Accept(this);
        }

        public void Visit(AndOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" {op.ToString()} ");
            op.RightOperand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(OrOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" {op.ToString()} ");
            op.RightOperand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(TagNode tag)
        {
            _resultBuilder.Append(tag.ToString());
        }

        public void Visit(BooleanNode boolean)
        {
            _resultBuilder.Append(boolean.ToString());
        }
    }
}
