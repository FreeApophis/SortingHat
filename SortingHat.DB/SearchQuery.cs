using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.DB;

/// <summary>
/// Translates a parsed search expression into SQL. This is a pure function: the tags of the expression
/// are resolved beforehand and passed in as the names of the SQL parameters holding their ids.
/// </summary>
internal static class SearchQuery
{
    private const string Select = """
        SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path
        FROM Files
        JOIN FilePaths ON FilePaths.FileId = Files.Id
        """;

    public static IEnumerable<string> TagNames(ParseNode expression)
        => expression switch
        {
            AndOperatorNode and => TagNames(and.LeftOperand).Concat(TagNames(and.RightOperand)),
            OrOperatorNode or => TagNames(or.LeftOperand).Concat(TagNames(or.RightOperand)),
            NotOperatorNode not => TagNames(not.Operand),
            BooleanNode => [],
            TagNode tag => [tag.Tag],
        };

    /// <param name="expression">The parsed search expression.</param>
    /// <param name="tagParameters">The SQL parameter holding the tag id for every known tag name, unknown tags are missing.</param>
    /// <returns>The query selecting the files matching the expression.</returns>
    public static string Sql(ParseNode expression, IReadOnlyDictionary<string, string> tagParameters)
        => $"""
            {Select}
            WHERE {Condition(expression, tagParameters)}
            ORDER BY Files.Hash, FilePaths.Path
            """;

    private static string Condition(ParseNode expression, IReadOnlyDictionary<string, string> tagParameters)
        => expression switch
        {
            AndOperatorNode andNode => $"({Condition(andNode.LeftOperand, tagParameters)} AND {Condition(andNode.RightOperand, tagParameters)})",
            OrOperatorNode orNode => $"({Condition(orNode.LeftOperand, tagParameters)} OR {Condition(orNode.RightOperand, tagParameters)})",
            NotOperatorNode notNode => $"NOT ({Condition(notNode.Operand, tagParameters)})",
            BooleanNode boolean => boolean.BoolConstant ? "1" : "0",

            // an unknown tag is not on any file
            TagNode tag => tagParameters.GetValueOrNone(tag.Tag).Match(none: "0", some: TaggedWithin),
        };

    /// <summary>
    /// The file is tagged with the tag or one of its descendants. UNION (not UNION ALL) terminates even on cyclic tag trees.
    /// </summary>
    private static string TaggedWithin(string tagParameter)
        => $"""
            EXISTS (
                WITH RECURSIVE Subtree(Id) AS (SELECT {tagParameter} UNION SELECT Tags.Id FROM Tags JOIN Subtree ON Tags.ParentId = Subtree.Id)
                SELECT 1 FROM FileTags WHERE FileTags.FileId = Files.Id AND FileTags.TagId IN (SELECT Id FROM Subtree))
            """;
}
