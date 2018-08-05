using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SortingHat.API.Parser
{
    public class Tokenizer
    {
        private StringReader _reader;

        public IEnumerable<Token> Scan(string expression)
        {
            _reader = new StringReader(expression);

            /// Or         := "or" | "||" | "∨"
            /// And        := "and" | "&&"| "∧"
            /// Not        := "not" | "!" | "¬"
            /// True       := "true" | "1"
            /// False      := "false" | "0"
            while (_reader.Peek() != -1)
            {
                var c = (char)_reader.Peek();
                if (char.IsWhiteSpace(c))
                {
                    _reader.Read();
                    continue;
                }

                if (c == ':')
                {
                    var value = GetNextIdentifier();
                    yield return new TagToken(value);
                }

                if (c == '∨')
                {
                    _reader.Read();
                    yield return new OrToken();
                }
                if (c == '∧')
                {
                    _reader.Read();
                    yield return new AndToken();
                }
                if (c == '¬')
                {
                    _reader.Read();
                    yield return new NotToken();
                }

                else if (c == '(')
                {
                    _reader.Read();
                    yield return new OpenParenthesisToken();
                }
                else if (c == ')')
                {
                    _reader.Read();
                    yield return new ClosedParenthesisToken();
                }
                else if (char.IsLetter(c))
                {
                    var name = GetNextIdentifier();
                    switch (name.ToLower())
                    {
                        case "or":
                            yield return new OrToken();
                            break;
                        case "and":
                            yield return new AndToken();
                            break;
                        case "not":
                            yield return new NotToken();
                            break;
                        case "true":
                            yield return new BoolConstantToken(true);
                            break;
                        case "false":
                            yield return new BoolConstantToken(false);
                            break;
                        default:
                            break;
                            throw new Exception("Unknown Identifier...");
                    }
                } 
            }
        }

        private string GetNextIdentifier()
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && !char.IsWhiteSpace((char)_reader.Peek()))
            {
                var character = (char)_reader.Read();
                sb.Append(character);
            }

            return sb.ToString();
        }
    }
}
