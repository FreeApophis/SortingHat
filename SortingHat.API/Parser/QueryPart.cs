namespace SortingHat.API.Parser;

/// <summary>A part of a query that can come next, for a <see cref="ParseError"/> and for suggestions while typing.</summary>
public enum QueryPart
{
    Tag,
    And,
    Or,
    Not,
    OpenParenthesis,
    ClosedParenthesis,
}
