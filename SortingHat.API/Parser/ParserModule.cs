using Autofac;

namespace SortingHat.API.Parser
{
    public class ParserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExpressionParser>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).InstancePerLifetimeScope().AsSelf();
            builder.RegisterType<FactorParser>().AsSelf();
            builder.RegisterType<TermParser>().AsSelf();
            builder.Register(_ => LexerRules.Create()).SingleInstance();
            builder.RegisterType<Parser>().AsSelf();
        }
    }
}
