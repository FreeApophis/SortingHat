﻿using System;
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
                    var value = GetNextIdentifier(true);
                    yield return new TagToken(value);
                }

                if (c == '∨')
                {
                    _reader.Read();
                    yield return new OrToken();
                }
                if (c == '|')
                {
                    _reader.Read();
                    if ('|' == (char)_reader.Read())
                    {
                        yield return new AndToken();
                    }
                    else
                    {
                        throw new Exception("Single & is not a legal operator...");
                    }
                }

                if (c == '∧')
                {
                    _reader.Read();
                    yield return new AndToken();
                }
                if (c == '&')
                {
                    _reader.Read();
                    if ('&' == (char)_reader.Read())
                    {
                        yield return new AndToken();
                    }
                    else
                    {
                        throw new Exception("Single & is not a legal operator...");
                    }
                }

                if (c == '¬' || c == '!')
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
                    var name = GetNextIdentifier(false);
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

        private bool IsLegalIdentifier(bool tag)
        {
            var current = (char)_reader.Peek();
            return char.IsLetter(current)
                || char.IsNumber(current)
                || current == '-'
                || current == '_'
                || tag && current == ':';
        }

        private string GetNextIdentifier(bool tag)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && IsLegalIdentifier(tag))
            {
                var character = (char)_reader.Read();
                sb.Append(character);
            }

            return sb.ToString();
        }
    }
}