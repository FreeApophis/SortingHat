namespace SortingHat.API.DI;

/// <summary>How many values an <see cref="ArgumentDefinition"/> takes.</summary>
public enum Arity
{
    ExactlyOne,
    ZeroOrOne,
    ZeroOrMore,
    OneOrMore,
}
