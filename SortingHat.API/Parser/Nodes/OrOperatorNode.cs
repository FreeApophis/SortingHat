namespace SortingHat.API.Parser.Nodes;

/// <summary>The files at least one operand matches.</summary>
/// <param name="LeftOperand">The query left of 'or'.</param>
/// <param name="RightOperand">The query right of 'or'.</param>
public sealed record OrOperatorNode(ParseNode LeftOperand, ParseNode RightOperand) : ParseNode;
