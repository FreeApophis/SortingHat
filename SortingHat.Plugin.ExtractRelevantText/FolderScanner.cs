using System.Collections.Generic;
using System.IO;
using apophis.Lexer;
using JetBrains.Annotations;
using SortingHat.ConsoleWriter;
using SortingHat.Plugin.ExtractRelevantText.Token;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public class FolderScanner
    {
        private readonly IConsoleWriter _consoleWriter;
        private TokenWalker _tokenWalker;

        [UsedImplicitly]
        public FolderScanner(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
            var lexerRules = new LexerRules();
            var tokenizer = new Tokenizer(lexerRules, e => new LexerReader(e));
            _tokenWalker = new TokenWalker(tokenizer, () => new EpsilonToken());
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
                _consoleWriter.WriteLine(_tokenWalker.Pop().Token.ToString());
            }

        }
    }


}