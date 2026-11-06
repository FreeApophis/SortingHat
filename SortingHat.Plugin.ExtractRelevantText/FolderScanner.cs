using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Lexer;
using Funcky.Lexer.Extensions;
using Funcky.Lexer.Token;
using JetBrains.Annotations;
using SortingHat.Plugin.ExtractRelevantText.Token;

namespace SortingHat.Plugin.ExtractRelevantText;

[method: UsedImplicitly]
public class FolderScanner(IConsoleWriter consoleWriter)
{
    private readonly LexerRuleBook _lexerRuleBook = LexerRules.Create();
    private readonly Dictionary<string, int> _wordCount = [];

    internal bool Scan(IEnumerable<string> folders)
    {
        consoleWriter.WriteLine("Scanning...");

        folders
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
            .ForEach(Read);

        return true;
    }

    private void Read(string file)
    {
        consoleWriter.WriteLine($"Reading: {file}");

        var walker = _lexerRuleBook.Scan(File.ReadAllText(file)).Walker;

        while (!walker.NextIs<EpsilonToken>())
        {
            var lexeme = walker.Pop();

            Count(lexeme.Token);
        }

        PrintSignificant();
    }

    private void PrintSignificant()
    {
        consoleWriter.WriteLine($"Words: {_wordCount.Count}");

        _wordCount
            .OrderByDescending(w => w.Value)
            .Select(w => w.Key)
            .Take(500)
            .ForEach(significantWord => consoleWriter.WriteLine(significantWord));
    }

    private void Count(IToken token)
    {
        if (token is WordToken word)
        {
            _wordCount[word.Word] = _wordCount
                .GetValueOrNone(key: word.Word)
                .Match(none: 1, some: count => count + 1);
        }
    }
}