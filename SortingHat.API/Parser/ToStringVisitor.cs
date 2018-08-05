using System;
using System.Collections.Generic;
using System.Text;
using SortingHat.API.Parser.Nodes;

namespace SortingHat.API.Parser
{
    public class ToStringVisitor : INodeVisitor
    {
        public string Result => _resultBuilder.ToString();
        private StringBuilder _resultBuilder = new StringBuilder();

        public void Visit(UnaryOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(BinaryOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(AndOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(OrOperatorNode op)
        {
            throw new NotImplementedException();
        }

        public void Visit(TagNode number)
        {
            throw new NotImplementedException();
        }

        public void Visit(BooleanNode boolean)
        {
            throw new NotImplementedException();
        }

        public void Visit(IParseNode op)
        {
            throw new NotImplementedException();
        }
    }
}
