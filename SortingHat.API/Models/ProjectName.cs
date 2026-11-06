using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>
/// A validated project name. A project is stored in the file <c>{name}.db</c> next to the main database, so its name must be a safe file name of its own.
/// </summary>
public sealed record ProjectName
{
    private const int MaximumLength = 64;

    private static readonly ImmutableHashSet<string> WindowsDeviceNames = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"]);

    private ProjectName(string value)
        => Value = value;

    public string Value { get; }

    /// <summary>Returns the project name, or why <paramref name="name"/> cannot be one.</summary>
    public static Either<string, ProjectName> Parse(string name, string mainDatabaseName)
        => Problem(name, mainDatabaseName)
            .Match(
                none: () => Either<string, ProjectName>.Right(new ProjectName(name)),
                some: Either<string, ProjectName>.Left);

    /// <summary>Returns the name for a new project, or why <paramref name="name"/> cannot be one, also when an existing project has it in any case.</summary>
    public static Either<string, ProjectName> ParseNew(string name, IEnumerable<string> existingProjects, string mainDatabaseName)
        => Parse(name, mainDatabaseName)
            .SelectMany(projectName => NotTaken(projectName, existingProjects));

    public override string ToString()
        => Value;

    private static Option<string> Problem(string name, string mainDatabaseName)
        => name switch
        {
            "" => "A project name cannot be empty.",
            { Length: > MaximumLength } => $"The project name '{name}' is too long, use at most {MaximumLength} characters.",
            _ when !name.All(IsAllowed) => $"The project name '{name}' can only contain letters, digits, '-' and '_'.",
            _ when WindowsDeviceNames.Contains(name) => $"'{name}' is reserved by Windows and cannot be a project name.",
            _ when IsSameName(name, mainDatabaseName) => $"'{name}' is the name of the main database and cannot be a project name.",
            _ => Option<string>.None,
        };

    private static Either<string, ProjectName> NotTaken(ProjectName projectName, IEnumerable<string> existingProjects)
        => existingProjects
            .FirstOrNone(project => IsSameName(projectName.Value, project))
            .Match(
                none: () => Either<string, ProjectName>.Right(projectName),
                some: project => Either<string, ProjectName>.Left($"The project name '{projectName}' is already taken by the project '{project}'."));

    private static bool IsAllowed(char character)
        => char.IsLetterOrDigit(character) || character is '-' or '_';

    /// <summary>Windows file names ignore case, so <c>default</c> and <c>Default</c> would share one database file.</summary>
    private static bool IsSameName(string name, string other)
        => string.Equals(name, other, StringComparison.OrdinalIgnoreCase);
}
