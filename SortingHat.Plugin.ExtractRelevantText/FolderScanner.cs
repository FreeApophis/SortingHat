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
            static ILinePositionCalculator NewLinePositionCalculator(List<Lexem> lexems) => new LinePositionCalculator(lexems);
            static ILexerReader NewLexerReader(string expression) => new LexerReader(expression);
            var tokenizer = new Tokenizer(lexerRules, NewLexerReader, NewLinePositionCalculator);
            static IToken NewEpsilonToken() => new EpsilonToken();
            _tokenWalker = new TokenWalker(tokenizer, NewEpsilonToken, NewLinePositionCalculator);
            _wordCount = new Dictionary<string, int>();
        }

        internal bool Scan(IEnumerable<string> folders)
        {
            _consoleWriter.WriteLine("Scanning...");

            folders
                .SelectMany(folder => Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
                .Each(Read);

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
            var signifcantWords = _wordCount
                .OrderByDescending(w => w.Value)
                .Select(w => w.Key)
                .Take(500);
            _consoleWriter.WriteLine($"Words: {_wordCount.Count}");
            foreach (var signifcantWord in signifcantWords)
            {
                _consoleWriter.WriteLine(signifcantWord);
            }
        }

        private void Count(IToken token)
        {
            if (token is WordToken word)
            {
                if (_wordCount.TryGetValue(word.Word, out var count))
                {
                    _wordCount[word.Word] = count + 1;
                }
                else
                {
                    _wordCount[word.Word] = 1;
                }
            }
        }
    }


}