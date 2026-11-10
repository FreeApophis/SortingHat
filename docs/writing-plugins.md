# Writing plugins

Most of what SortingHat can do comes from plugins. A plugin can add commands to `hat`, variables for tag masks, and handlers that react when something happens to a file. This guide shows how to build one, how hat loads it, and what to watch out for.

The plugins in this repository are working examples:

| Plugin | Shows |
|---|---|
| `SortingHat.Plugin.FileType` | tag variables (`{FileType.Category}`), a command (`identify`), an embedded resource |
| `SortingHat.Plugin.Exif` | tag variables from a third-party library (MetadataExtractor), a command with file patterns |
| `SortingHat.Plugin.MoveOnTag` | a file event handler, commands that keep settings per project |
| `SortingHat.Plugin.ExtractRelevantText` | a command with its own services |

## The project

A plugin is a class library that references `SortingHat.API` and nothing else of hat:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <!-- the build output is a plugin folder: the plugin with all its own dependencies -->
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <!-- the host provides SortingHat.API and its dependencies, the plugin must not ship its own copy -->
    <ProjectReference Include="..\SortingHat.API\SortingHat.API.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>

</Project>
```

- `EnableDynamicLoading` copies every dependency of the plugin into its build output.
- `Private=false` and `ExcludeAssets=runtime` keep `SortingHat.API` and its dependencies out of it. hat brings them itself (see [Versions](#versions-and-compatibility)).
- The `Version` of the assembly is the version `hat plugins` shows.

## The module

Every plugin has at least one class that is both an Autofac `Module` and an `IPlugin`. hat finds it, describes the plugin with it, and lets it register its services:

```csharp
using Autofac;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.Plugin.Example;

public class ExampleModule : Module, IPlugin
{
    public string Name => "Example";

    public string Description => "Shows how a plugin is built, with the hello command.";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<HelloCommand>().As<IDeclaredCommand>();
    }
}
```

Your services are ordinary classes with constructor injection. They can depend on each other, on anything hat registers (see [Services](#services-from-hat)), and on the services of other plugins.

**`Load` runs twice.** hat first runs every module of a plugin into a throwaway container, to find out whether the plugin loads at all. Only then does it register the module for real. So `Load` must only register things:

- No I/O, no reading files, no connecting anywhere. Do that in your services, when they are used.
- Prefer `RegisterType<T>()` over `RegisterInstance(...)`. An instance you register from a field of the module is shared by both runs, and the throwaway container never disposes it.

## Installing a plugin

hat loads plugins from the `plugins` folder next to `hat.exe`. Every plugin has a folder of its own, named like its assembly, with everything it needs:

```
hat.exe
plugins/
  SortingHat.Plugin.Example/
    SortingHat.Plugin.Example.dll
    SomeLibrary.dll
```

`dotnet publish <plugin project> --output <hat folder>/plugins/<plugin name>` builds exactly that folder. `Scripts/publish.ps1` and `Scripts/publish.sh` publish hat with every `SortingHat.Plugin.*` project in this repository.

**A plugin loads completely or not at all.** Whatever goes wrong becomes a warning, and hat runs without that plugin:

```
Warning: plugin 'SortingHat.Plugin.Example' not loaded: it was built for SortingHat.API 3.x, hat has 2.0.0.
```

That includes a DLL lying directly in `plugins`, a folder without its assembly, a file that is no assembly, an assembly without a plugin module, an incompatible version, and an exception in a module's constructor or in `Load`.

## Versions and compatibility

Every plugin gets its own `AssemblyLoadContext`, so it can bring its own versions of its own libraries. A few assemblies are **contract assemblies**: they always come from hat, so a plugin and hat share their types.

- `SortingHat.API`
- `Apophis.CLI`, `Apophis.FileSystem`, `Apophis.Utils`
- `Autofac`
- `Funcky`, `Funcky.Lexer`
- `Serilog`

Before hat touches a plugin's types, it checks what the plugin was built against:

- Every contract assembly must have hat's **major** version and at most hat's **minor** version. A plugin built against `SortingHat.API` 2.0 runs on hat 2.3, but not on hat 3.0, and not on hat 2.0 if it was built against 2.1.
- `System.Runtime` must not be newer than the .NET that hat runs on.

`SortingHat.API` follows semantic versioning: new members and new extension points are a minor version, changes and removals a major version. hat's version is in `SortingHat.API/SortingHat.API.csproj`; `hat version` shows hat's own.

Don't rely on anything else hat happens to have.

## Extension points

### Commands: `IDeclaredCommand`

A command declares its name, arguments and options. hat parses and checks the command line, writes the help (`hat help <command>`, `hat <command> --help`) and only then runs the command:

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.DI;

namespace SortingHat.Plugin.Example;

internal sealed class HelloCommand(IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Who = new() { Name = "who", Description = "The one to greet." };

    private static readonly OptionDefinition Loud = new() { Name = "loud", ShortName = "l", Description = "Greets in capitals." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "hello";
    public IReadOnlyList<string> Aliases => ["hi"];
    public string Description => "Greets someone.";
    public string Details => "A longer explanation for the help of this command, shown after the description.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Who];
    public IReadOnlyList<OptionDefinition> Options => [Loud];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(Who).Match(none: false, some: who => Greet(who, input.Has(Loud))));

    private bool Greet(string who, bool loud)
    {
        consoleWriter.WriteLine(loud ? $"HELLO, {who.ToUpperInvariant()}!" : $"Hello, {who}!");

        return true;
    }
}
```

- **Arguments** come in their order on the command line; `Arity` says how many values each takes, and only the last one should take several. Read them with `input.Value(...)` (one value, as an `Option<string>`) or `input.Values(...)`. **Options** are flags; `input.Has(...)` tells whether one was given.
- **The result** decides the exit code: `true` is success, `false` is 1. Report the reason before you return `false`.
- **The description** is one sentence that starts with a capital letter and ends with a period; the overview of `hat help` shows it. hat's tests check this for its own commands and for the plugins in this repository.
- **Names are first come, first served.** hat's own commands come first, then the plugins in the order they load. When your command's name is taken, hat leaves it out; when only an alias is taken, hat drops that alias. Both with a warning, so check `hat help` after installing.
- **Output** goes through `IConsoleWriter`: plain lines, written as they are. hat renders its own tables with Spectre.Console, but that is not part of the contract. In the desktop app the lines appear in the message panel at the bottom of the window, and a plugin may write from any thread.
- **Questions** go through `IUserPrompt`: `Confirm(question)` (only 'y' is yes, Enter is no) and `Choose(question, choices, describe)` (arrow keys). Both return none when hat cannot ask, such as in a script: decide what happens then, and say why, like `remove-project`, which keeps the project and names its `--yes` option. The desktop app cannot ask yet, so there both always return none.
- **File arguments:** `IFilePathExtractor.FromFilePatterns(patterns, recursive)` resolves files, wildcards and (with `recursive`) folders the way hat's own commands do, and returns the problems on the left when an argument matches nothing. Offer a `recursive` option (`-r`) for it, like the `exif` command.

### Tag variables: `IAutoTag`

A tag variable fills a tag mask per file, such as `hat tag-files :Photos:{Camera.Make} *.jpg`:

```csharp
using Funcky.Monads;
using SortingHat.API.AutoTag;

namespace SortingHat.Plugin.Example;

internal sealed class ExtensionAutoTag : IAutoTag
{
    public string Name => "Example.Extension";

    public AutoTagParameter Parameter => AutoTagParameter.None;

    public string Description => "The extension of the file name, such as pdf.";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => file.Info.Extension.TrimStart('.') switch
        {
            "" => Option<string>.None,
            var extension => Option.Some(extension),
        };
}
```

- **The name** starts with your plugin's prefix (`FileType.`, `Camera.`): two variables must not share a name. hat checks every mask against all variables before it touches a file, and reports an unknown variable or a wrong argument.
- **The parameter** says what may follow the name: `AutoTagParameter.None`, `Integer` (`{Folder.FromRoot.2}`) or `OneOf(["Year", "Month", ...])` (`{CreatedAt.Year}`). `argument` holds what the user wrote. `DateAutoTag` is a base class for date variables with the usual parts.
- **The value** is `Option<string>`: none when the file has no such value, such as a PDF and a camera. hat then skips that tag for that file and says so in one line per mask. A value is made into a valid tag level for you (characters a tag cannot have become `-`). An exception also skips the tag, with the reason.
- **Read once:** when several of your variables need the same expensive fact, read it with `file.Remember(key, info => ...)`; every variable gets the same result for that file.

Register it as `builder.RegisterType<ExtensionAutoTag>().As<IAutoTag>()`; `hat help` lists it with its description, and `hat file-info --variables <file>` shows its value for a file (for a numbered variable, every number up to the last one with a value).

### File events: `IFileEventHandler<TEvent>`

A handler hears about a change after hat has stored it. There is one handler interface per event:

```csharp
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.Events;

namespace SortingHat.Plugin.Example;

internal sealed class TaggedFileLogger(IConsoleWriter consoleWriter) : IFileEventHandler<FileTagged>
{
    public Task Handle(FileTagged fileEvent, CancellationToken cancellationToken)
    {
        consoleWriter.WriteLine($"{fileEvent.File.Path} got {fileEvent.Tags.Count} tags.");

        return Task.CompletedTask;
    }
}
```

Register it as `builder.RegisterType<TaggedFileLogger>().As<IFileEventHandler<FileTagged>>()`. One class can handle several events.

| Event | When |
|---|---|
| `FileAdded(FileAt File)` | `tag-files` stored a file hat did not know: new content, or known content at a new path |
| `FileChanged(FileAt Stored, FileAt Current)` | `tag-files` or `repair --rehash` stored new content at a known path; the tags were carried over |
| `FileTagged(FileAt File, IReadOnlyList<Tag> Tags)` | `tag-files` stored the tags of a file: once per file, with all tags it got in that run |
| `FileUntagged(FileAt File, IReadOnlyList<Tag> Tags)` | `untag-files` removed tags from a file: once per file, with all tags of that run |
| `FileMoved(FileAt File, string Destination)` | `move-files` moved a file, on disk and in the database |

Within one file, `FileAdded` or `FileChanged` comes before `FileTagged`. A dry run raises nothing, and neither does `copy-files`: a copy is not stored.

- hat calls the handlers one after the other and awaits each one.
- **A handler that throws** becomes a warning that names it; the change stays, and the other handlers still run.
- Handlers only hear about changes hat's commands make. A handler that tags or moves files itself does not raise new events, so handlers cannot call each other in a loop.
- MoveOnTag is the complete example: it moves a tagged file and tells hat the new path with `IFile.MovePath`.

## Services from hat

Inject these into your services; they are the same ones hat's own commands use. Everything that works with data uses the active project.

| Service | For |
|---|---|
| `IFile` | the files hat knows: find a path, tag and untag, paths and names, search, duplicates, record a move |
| `ITag` | the tags: store, rename, move, remove, list, count files |
| `IFileScanner` | what a path is: new, known or changed since it was stored, with its hash |
| `IHashService` | the content hash of a file |
| `IFilePathExtractor` | files, wildcards and folders from command line arguments |
| `IProjects` | the projects: list, create, remove, export, import |
| `IPluginSettings<TPlugin>` | your plugin's own settings: `Global` for every project, `Project` for the active project |
| `IConsoleWriter` | output lines |
| `IUserPrompt` | yes/no questions and choices, none when hat cannot ask |
| `Serilog.ILogger` | hat's log; the logger you get already names your class, such as `SortingHat.Plugin.MoveOnTag.MoveOnTagHandler` |
| `ICopyFile`, `IMoveFile`, `IExistsFile`, `IExistsDirectory` | the file system, replaceable in tests |

- Values with rules have their own types: `Tag.Parse(":tax:2018")`, `TagName`, `Hash.Parse(...)`, `ProjectName.Parse(...)` return `Either<string, T>` with the reason on the left. Parse user input once, then work with the type.
- **Logging** goes to hat's log file (`logs` next to the databases, a file per day). Use message templates, `logger.Information("Moved {Source} to {Target}", source, target)`, not interpolated strings. Information is written by default; the user can change the level in `hat-config.json` (`"Log": { "MinimumLevel": "Warning" }`). Show the user what matters on the console as well: nobody reads the log unless something went wrong.
- **Settings** come from `IPluginSettings<YourModule>`. hat keeps them apart from its own settings and from every other plugin, so any key is yours, such as `Rules`. Use `Project` for whatever names tags or files, because those belong to a project; `Global` for the rest. Values are strings; store structured values as JSON.

## Testing

hat's own plugins are tested through the real application, the way a user runs them:

- `SortingHat.Test` builds every plugin into `PluginFixtures/Working/<plugin>/` (see the `CopyPluginFixtures` target in its project file).
- `HatSandbox.WithPluginFixtures("Working")` is the real `hat` with those plugins, a real SQLite database and real files in a temporary folder. A test runs commands with `Hat([...])` and checks `Lines`, `Rows`, the files and the database.
- `MoveOnTagTest` and `FileTypeTest` are complete examples. Keep the logic of your plugin in pure functions where you can (like `MoveRules` in MoveOnTag); those need no sandbox at all.
