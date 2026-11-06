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

public class FolderScanner
{
    private readonly IConsoleWriter _consoleWriter;
    private readonly LexerRuleBook _lexerRuleBook;
    private readonly Dictionary<string, int> _wordCount;

    [UsedImplicitly]
    public FolderScanner(IConsoleWriter consoleWriter)
    {
        _consoleWriter = consoleWriter;
        _lexerRuleBook = LexerRules.Create();
        _wordCount = new Dictionary<string, int>();
    }

    internal bool Scan(IEnumerable<string> folders)
    {
        _consoleWriter.WriteLine("Scanning...");

        folders
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
            .ForEach(Read);

        return true;
    }

    private void Read(string file)
    {
        _consoleWriter.WriteLine($"Reading: {file}");

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
        _consoleWriter.WriteLine($"Words: {_wordCount.Count}");

        _wordCount
            .OrderByDescending(w => w.Value)
            .Select(w => w.Key)
            .Take(500)
            .ForEach(significantWord => _consoleWriter.WriteLine(significantWord));
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