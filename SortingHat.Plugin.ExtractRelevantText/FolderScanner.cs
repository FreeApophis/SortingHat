using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.Lexer;
using apophis.Lexer.Tokens;
using JetBrains.Annotations;
using SortingHat.CliAbstractions;
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
            var tokenizer = new Tokenizer(lexerRules, e => new LexerReader(e));
            _tokenWalker = new TokenWalker(tokenizer, () => new EpsilonToken());
            _wordCount = new Dictionary<string, int>();
        }

        internal bool Scan(IEnumerable<string> folders)
        {
            _consoleWriter.WriteLine("Scanning...");
            foreach (var folder in folders)
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    Read(file);
                }
            }

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
                } else
                {
                    _wordCount[word.Word] = 1;
                }
            }
        }
    }


}