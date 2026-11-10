namespace SortingHat.API.Parser.Nodes;

/// <summary>The files both operands match.</summary>
/// <param name="LeftOperand">The query left of 'and'.</param>
/// <param name="RightOperand">The query right of 'and'.</param>
public sealed record AndOperatorNode(ParseNode LeftOperand, ParseNode RightOperand) : ParseNode;
