namespace SortingHat.API.Parser.Nodes;

/// <summary>Every file ('true') or none ('false').</summary>
/// <param name="BoolConstant">The constant.</param>
public sealed record BooleanNode(bool BoolConstant) : ParseNode;
