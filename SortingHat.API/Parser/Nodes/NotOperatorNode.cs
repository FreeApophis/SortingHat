namespace SortingHat.API.Parser.Nodes;

/// <summary>The files the operand does not match.</summary>
/// <param name="Operand">The negated query.</param>
public sealed record NotOperatorNode(ParseNode Operand) : ParseNode;
