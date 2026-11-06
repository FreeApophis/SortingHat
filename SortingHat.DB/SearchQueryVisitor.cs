using System;
using System.Text;
using JetBrains.Annotations;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using SortingHat.DB.Access;

namespace SortingHat.DB;

[UsedImplicitly]
public class SearchQueryVisitor : INodeVisitor
{
    private const string GroupAndOrderClause = "\r\nGROUP BY FilePaths.Id\r\nORDER BY Files.Hash";
    private const string WhereStart = "WHERE ";
    private readonly StringBuilder _selectBuilder = new StringBuilder();

    private readonly StringBuilder _whereBuilder = new StringBuilder();
    private readonly SQLiteTag _sqLiteTag;
    private readonly ITagParser _tagParser;
    private int _fileTagCount;

    public SearchQueryVisitor(SQLiteTag sqLiteTag, ITagParser tagParser)
    {
        _sqLiteTag = sqLiteTag;
        _tagParser = tagParser;

        _selectBuilder.AppendLine("SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path");
        _selectBuilder.AppendLine("FROM Files");
        _selectBuilder.AppendLine("JOIN FilePaths ON FilePaths.FileId = Files.Id");

        _whereBuilder.Append(WhereStart);
    }

    public string Result => _selectBuilder + WhereClause + GroupAndOrderClause;

    public bool UnknownTag { get; private set; }

    private string WhereClause => _whereBuilder.Length == WhereStart.Length ? $"{WhereStart}0" : _whereBuilder.ToString();

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

    public void Visit(TagNode tag)
    {
        long? tagId = GetTagId(tag);

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

    public void Visit(BooleanNode boolean)
    {
        _whereBuilder.Append(boolean.BoolConstant ? "1" : "0");
    }

    private long? GetTagId(TagNode tagNode)
    {
        var tag = _tagParser.Parse(tagNode.Tag);

        return tag == null
            ? null
            : _sqLiteTag.Find(tag);
    }
}
