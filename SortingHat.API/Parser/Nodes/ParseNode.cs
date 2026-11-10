using SortingHat.API.Parser.OperatorType;

namespace SortingHat.API.Parser.Nodes;

/// <summary>
/// A parsed search query. The cases are closed: only this assembly can add one, so a switch over them is exhaustive without a default case,
/// and the compiler finds every switch a new case has to be added to.
/// </summary>
public closed record ParseNode
{
    /// <summary>The query as text hat parses again, with parentheses around every 'and' and 'or', such as '(:a or (:b and not(true)))'.</summary>
    public sealed override string ToString()
        => ToString(new TextOperatorType());

    internal string ToString(IOperatorType operators)
        => this switch
        {
            TagNode tag => tag.Tag,
            BooleanNode boolean => boolean.BoolConstant ? "true" : "false",
            NotOperatorNode not => $"{operators.Not}{operators.NotOpen}{not.Operand.ToString(operators)}{operators.NotClose}",
            AndOperatorNode and => $"({and.LeftOperand.ToString(operators)} {operators.And} {and.RightOperand.ToString(operators)})",
            OrOperatorNode or => $"({or.LeftOperand.ToString(operators)} {operators.Or} {or.RightOperand.ToString(operators)})",
        };
}
