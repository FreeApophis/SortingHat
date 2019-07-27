using SortingHat.API.Models;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser;
using System.Text;
using System;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    [UsedImplicitly]
    public class SearchQueryVisitor : INodeVisitor
    {
        public string Result => _selectBuilder.ToString() + _whereBuilder + GroupBy;
        private readonly StringBuilder _selectBuilder = new StringBuilder();
        private readonly StringBuilder _whereBuilder = new StringBuilder();
        private const string GroupBy = "\r\nGROUP BY FilePaths.Id";
        private readonly IProjectDatabase _db;
        private readonly ITagParser _tagParser;
        private int _fileTagCount;

        public bool UnknownTag { get; private set; }

        public SearchQueryVisitor(IProjectDatabase db, ITagParser tagParser)
        {
            _db = db;
            _tagParser = tagParser;

            _selectBuilder.AppendLine("SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path");
            _selectBuilder.AppendLine("FROM Files");
            _selectBuilder.AppendLine("JOIN FilePaths ON FilePaths.FileId = Files.Id");

            _whereBuilder.Append("WHERE ");
        }

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
            _whereBuilder.Append("NOT (");
            op.Operand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(AndOperatorNode op)
        {
            _whereBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _whereBuilder.Append(" AND ");
            op.RightOperand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(OrOperatorNode op)
        {
            _whereBuilder.Append("(");
            op.LeftOperand.Accept(this);
            _whereBuilder.Append(" OR ");
            op.RightOperand.Accept(this);
            _whereBuilder.Append(")");
        }

        public void Visit(TagNode tagNode)
        {
            long? tagId = GetTagId(tagNode);

            if (tagId.HasValue)
            {
                _selectBuilder.AppendLine($"JOIN FileTags ft{_fileTagCount} ON ft{_fileTagCount}.FileId = Files.Id");
                _whereBuilder.Append($"ft{_fileTagCount}.TagId = {tagId.Value}");
                _fileTagCount++;
            }
            else
            {
                // this query includes  a tag which does not exist.
                // This is usally not useful, but the query can still have a result,
                // because this tag will not participate in the query.
                UnknownTag = true;

                // Emitting false because the tag does not exist in the database
                _whereBuilder.Append(0);
            }
        }

        private long? GetTagId(TagNode tagNode)
        {
            var tag = _tagParser.Parse(tagNode.Tag);

            return tag == null
                ? null
                : ((SQLiteTag)_db.Tag).Find(tag);
        }

        public void Visit(BooleanNode boolean)
        {
            _whereBuilder.Append(boolean.BoolConstant ? "1" : "0");
        }
    }
}
