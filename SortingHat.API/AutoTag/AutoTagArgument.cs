using System.Globalization;
using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>The argument of a variable in a mask, already checked against the <see cref="AutoTagParameter"/> of its <see cref="IAutoTag"/>.</summary>
public sealed record AutoTagArgument
{
    private AutoTagArgument(Option<string> text, Option<int> number)
        => (Text, Number) = (text, number);

    /// <summary>The argument of a variable without one.</summary>
    public static AutoTagArgument None { get; } = new(Option<string>.None, Option<int>.None);

    /// <summary>The argument as it was written, such as <c>Year</c> or <c>2</c>.</summary>
    public Option<string> Text { get; }

    /// <summary>The argument of an <see cref="AutoTagParameter.Integer"/> variable.</summary>
    public Option<int> Number { get; }

    internal static AutoTagArgument OfText(string text)
        => new(text, Option<int>.None);

    internal static AutoTagArgument OfNumber(int number)
        => new(number.ToString(CultureInfo.InvariantCulture), number);
}
