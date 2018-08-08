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
                return _selectBuilder.ToString() + _whereBuilder.ToString() + GroupBy;
            }
        }
        private StringBuilder _selectBuilder = new StringBuilder();
        private StringBuilder _whereBuilder = new StringBuilder();
        private const string GroupBy = "\nGROUP BY FilePaths.ID";
        private SQLiteDB _db;
        private int _fileTagCount = 0;

        public SearchQueryVisitor(SQLiteDB db)
        {
            _db = db;

            _selectBuilder.AppendLine("SELECT FilePaths.Path, Files.Hash, Files.ID");
            _selectBuilder.AppendLine("FROM Files");
            _selectBuilder.AppendLine("JOIN FilePaths ON FilePaths.FileID = Files.ID");

            _whereBuilder.Append("WHERE ");
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
            _whereBuilder.Append("NOT (");
            op.Operand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(AndOperatorNode op)
        {
            _whereBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _whereBuilder.Append($" AND ");
            op.RightOperand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(OrOperatorNode op)
        {
            _whereBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _whereBuilder.Append($" OR ");
            op.RightOperand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(TagNode tagNode)
        {
            long? tagID = GetTagID(tagNode);

            if (tagID.HasValue)
            {
                _selectBuilder.AppendLine($"JOIN FileTags ft{_fileTagCount} ON ft{_fileTagCount}.FileID = Files.ID");
                _whereBuilder.Append($"ft{_fileTagCount}.TagID = {tagID.Value}");
                _fileTagCount++;
            }
            else
            {
                // Emitting false because the tag does not exist in the database
                _whereBuilder.Append(0);
            }
        }

        private long? GetTagID(TagNode tagNode)
        {
            var tag = Tag.Parse(tagNode.Tag);

            return ((SQLiteTag)_db.Tag).Find(tag);
        }

        public void Visit(BooleanNode boolean)
        {
            _whereBuilder.Append(boolean.BoolConstant ? "1" : "0");
        }
    }
}
