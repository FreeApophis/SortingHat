using Autofac;

namespace SortingHat.API.Parser
{
    public class ParserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => Parser.Create()).As<Parser>();
        }
    }
}
