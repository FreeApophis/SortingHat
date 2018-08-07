using SortingHat.API.Models;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser;
using System.Text;
using System;

namespace SortingHat.DB
{
    class SearchQueryVisitor : INodeVisitor
    {
        public string Result
        {
            get
            {
                if (_done == false)
                {
                    _resultBuilder.Append("\n");
                    _resultBuilder.Append("GROUP BY FilePaths.ID");
                    _done = true;
                }

                return _resultBuilder.ToString();
            }
        }
        private StringBuilder _resultBuilder = new StringBuilder();
        private SQLiteDB _db;
        private bool _done = false;

        public SearchQueryVisitor(SQLiteDB db)
        {
            _db = db;

            _resultBuilder.AppendLine("SELECT FilePaths.Path, Files.Hash, Files.ID");
            _resultBuilder.AppendLine("FROM Files");
            _resultBuilder.AppendLine("JOIN FileTags ON FileTags.FileID = Files.ID");
            _resultBuilder.AppendLine("JOIN FilePaths ON FilePaths.FileID = Files.ID");
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

        public void Visit(TagNode tagNode)
        {
            var tag = Tag.Parse(tagNode.Tag);
            var tagID = ((SQLiteTag)_db.Tag).Find(tag);

            if (tagID.HasValue)
            {
                _resultBuilder.Append($"FileTags.TagID = {tagID.Value}");
            }
            else
            {
                // Emitting false because the tag does not exist in the database
                _resultBuilder.Append(0);
            }
        }

        public void Visit(BooleanNode boolean)
        {
            _resultBuilder.Append(boolean.BoolConstant ? "1" : "0");
        }
    }
}
