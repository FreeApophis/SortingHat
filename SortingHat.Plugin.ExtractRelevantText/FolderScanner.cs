using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI.Writer;
using apophis.Lexer;
using apophis.Lexer.Tokens;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.Plugin.ExtractRelevantText.Token;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public class FolderScanner
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly TokenWalker _tokenWalker;
        private readonly Dictionary<string, int> _wordCount;

        [UsedImplicitly]
        public FolderScanner(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
            var lexerRules = new LexerRules();
            var tokenizer = new Tokenizer(lexerRules, e => new LexerReader(e), lexems => new LinePositionCalculator(lexems));
            _tokenWalker = new TokenWalker(tokenizer, () => new EpsilonToken(), lexems => new LinePositionCalculator(lexems));
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

            _tokenWalker.Scan(File.ReadAllText(file));

            while (!(_tokenWalker.Peek().Token is EpsilonToken))
            {
                var lexem = _tokenWalker.Pop();

                Count(lexem.Token);
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
                    .TryGetValue(key: word.Word)
                    .Match(none: 1, some: count => count + 1);
            }
        }
    }


}