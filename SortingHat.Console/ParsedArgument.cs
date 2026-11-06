using System;
using System.Collections.Generic;
using SortingHat.API.DI;
using Cli = System.CommandLine;

namespace SortingHat.CLI;

/// <summary>An argument of an <see cref="IDeclaredCommand"/> on the command line, with how to read its values from a parse result.</summary>
internal sealed record ParsedArgument(ArgumentDefinition Definition, Cli.Argument Argument, Func<Cli.ParseResult, IReadOnlyList<string>> Values);
