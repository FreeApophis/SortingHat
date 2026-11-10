namespace SortingHat.API.Parser.Nodes;

/// <summary>Files tagged with the tag or one of its descendants.</summary>
/// <param name="Tag">The tag as written in the query, such as ':tax:2018'.</param>
public sealed record TagNode(string Tag) : ParseNode;
