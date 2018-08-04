using System.Collections.Generic;

namespace SortingHat.API.Parser
{
    public class FunctionNode : IParseNode
    {
        public List<IParseNode> Parameters { get; } = new List<IParseNode>();
        public string Name { get; }

        public FunctionNode(string name)
        {
            Name = name;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
