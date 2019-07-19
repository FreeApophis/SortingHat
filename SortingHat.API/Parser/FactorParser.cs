using apophis.Lexer;
using SortingHat.API.Parser.Nodes;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public class FactorParser : IParser
    {
        private readonly ExpressionParser _expressionParser;

        public FactorParser(ExpressionParser expressionParser)
        {
            _expressionParser = expressionParser;
        }

        /// <summary>
        /// Parsing the following Production:
        /// Factor       := Tag | BoolConstant | Not Factor | "(" Expression ")
        /// </summary>
        public IParseNode Parse(TokenWalker walker)
        {
            if (walker.NextIs<TagToken>())
            {
                var lexem = walker.Pop();
                if (lexem.Token is TagToken tagToken && tagToken.Value != null)
                {
                    return new TagNode(tagToken.Value);
                }
            }

            if (walker.NextIs<BoolConstantToken>())
            {
                var lexem = walker.Pop();
                if (lexem.Token is BoolConstantToken boolToken)
                {
                    return new BooleanNode(boolToken.Value);
                }
            }

            if (walker.NextIs<NotToken>())
            {
                walker.Pop();
                return new NotOperatorNode(Parse(walker));
            }

            walker.ExpectOpeningParenthesis();
            var result = _expressionParser.Parse(walker);
            walker.ExpectClosingParenthesis();

            return result;
        }
    }
}