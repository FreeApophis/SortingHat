using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>The argument an <see cref="IAutoTag"/> takes after its name: none, a number, or one of a few choices.</summary>
public abstract record AutoTagParameter
{
    private AutoTagParameter()
    {
    }

    /// <summary>The variable is only its name: <c>{Camera.Make}</c>.</summary>
    public static AutoTagParameter None { get; } = new NoArgument();

    /// <summary>The variable takes a number from 0: <c>{Folder.FromRoot.2}</c>.</summary>
    public static AutoTagParameter Integer { get; } = new NumberArgument();

    /// <summary>The variable takes one of the <paramref name="choices"/>: <c>{CreatedAt.Year}</c>.</summary>
    public static AutoTagParameter OneOf(IReadOnlyList<string> choices)
        => new ChoiceArgument([.. choices]);

    /// <summary>How a variable with this parameter is written, for the help: <c>{CreatedAt.&lt;Year|Month&gt;}</c>.</summary>
    public abstract string Usage(string name);

    /// <summary>
    /// Every way to write the variable <paramref name="name"/>, without braces: <c>Camera.Make</c>; <c>CreatedAt.Year</c>, <c>CreatedAt.Month</c>, ...;
    /// for a number <c>Folder.FromRoot.0</c>, <c>Folder.FromRoot.1</c>, ... without end, so take only as many as needed.
    /// </summary>
    public abstract IEnumerable<string> Forms(string name);

    /// <summary>The argument given after the name, or why it does not fit, completing "the variable {...} ...".</summary>
    internal abstract Either<string, AutoTagArgument> Parse(Option<string> argument);

    private sealed record NoArgument : AutoTagParameter
    {
        public override string Usage(string name)
            => $"{{{name}}}";

        public override IEnumerable<string> Forms(string name)
            => [name];

        internal override Either<string, AutoTagArgument> Parse(Option<string> argument)
            => argument.Match(
                none: () => Either<string, AutoTagArgument>.Right(AutoTagArgument.None),
                some: _ => Either<string, AutoTagArgument>.Left("takes no argument"));
    }

    private sealed record NumberArgument : AutoTagParameter
    {
        public override string Usage(string name)
            => $"{{{name}.<number>}}";

        public override IEnumerable<string> Forms(string name)
            => Enumerable.Range(0, int.MaxValue).Select(number => $"{name}.{number}");

        internal override Either<string, AutoTagArgument> Parse(Option<string> argument)
            => argument
                .Where(text => text.All(char.IsAsciiDigit))
                .SelectMany(text => int.ParseOrNone(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .Match(
                    none: () => Either<string, AutoTagArgument>.Left("needs a number from 0 after its name, such as .0 or .2"),
                    some: number => Either<string, AutoTagArgument>.Right(AutoTagArgument.OfNumber(number)));
    }

    private sealed record ChoiceArgument(ImmutableArray<string> Choices) : AutoTagParameter
    {
        public override string Usage(string name)
            => $"{{{name}.<{string.Join("|", Choices)}>}}";

        public override IEnumerable<string> Forms(string name)
            => Choices.Select(choice => $"{name}.{choice}");

        internal override Either<string, AutoTagArgument> Parse(Option<string> argument)
            => argument
                .Where(Choices.Contains)
                .Match(
                    none: () => Either<string, AutoTagArgument>.Left($"needs one of {string.Join(", ", Choices)} after its name"),
                    some: choice => Either<string, AutoTagArgument>.Right(AutoTagArgument.OfText(choice)));
    }
}
