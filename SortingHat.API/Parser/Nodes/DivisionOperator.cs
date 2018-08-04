namespace SortingHat.API.Parser
{
    public class DivisionOperator : BinaryOperator
    {
        internal DivisionOperator(IParseNode leftOperand, IParseNode rightOperand) :
            base(leftOperand, rightOperand)
        {
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "/";
        }
    }
}