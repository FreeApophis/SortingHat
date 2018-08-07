using System;
using System.Text;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{

    public class ToStringVisitor : INodeVisitor
    {
        public ToStringVisitor(OperatorType operatorType = OperatorType.Text)
        {
            _operatorType = operatorType;
        }

        public string Result => _resultBuilder.ToString();
        private StringBuilder _resultBuilder = new StringBuilder();
        readonly private OperatorType _operatorType;

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
            _resultBuilder.Append(op.ToString(_operatorType));
            if (_operatorType == OperatorType.Text)
            {
                _resultBuilder.Append("(");
            }
            op.Operand.Accept(this);
            if (_operatorType == OperatorType.Text)
            {
                _resultBuilder.Append(")");
            }
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
            _resultBuilder.Append(tag.ToString());
        }

        public void Visit(BooleanNode boolean)
        {
            _resultBuilder.Append(boolean.ToString());
        }
    }
}
