﻿using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using System;
using System.Text;

namespace SortingHat.DB
{
    class SearchQueryVisitor : INodeVisitor
    {
        public string Result => _resultBuilder.ToString();
        private StringBuilder _resultBuilder = new StringBuilder();

        public SearchQueryVisitor()
        {
            _resultBuilder.Append("SELECT File.Hash, File.Size\n");
            _resultBuilder.Append("FROM File\n");
            _resultBuilder.Append("JOIN FileTags ON FileTags.FileID = File.ID\n");
            _resultBuilder.Append("JOIN Tags ON FileTags.TagID = ID\n");
            _resultBuilder.Append("WHERE ");
        }

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
            _resultBuilder.Append("NOT (");
            op.Operand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(AndOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" AND ");
            op.RightOperand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(OrOperatorNode op)
        {
            _resultBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _resultBuilder.Append($" OR ");
            op.RightOperand.Accept(this);
            _resultBuilder.Append(")");
        }

        public void Visit(TagNode tag)
        {
            _resultBuilder.Append($"Tags.ID = ToID({tag.Tag})");
        }

        public void Visit(BooleanNode boolean)
        {
            _resultBuilder.Append(boolean.BoolConstant ? "1" : "0");
        }
    }
}
