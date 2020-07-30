using apophis.Lexer;
using apophis.Lexer.Rules;
using Autofac;
using SortingHat.API.Parser.Token;

namespace SortingHat.API.Parser
{
    public class ParserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExpressionParser>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).InstancePerLifetimeScope().AsSelf();
            builder.RegisterType<FactorParser>().AsSelf();
            builder.RegisterType<TermParser>().AsSelf();
            builder.RegisterType<LexerRules>().As<ILexerRules>();
            builder.RegisterType<LexerReader>().As<ILexerReader>();
            builder.RegisterType<Tokenizer>().AsSelf();
            builder.Register(c => new TokenWalker(c.Resolve<Tokenizer>(), () => new EpsilonToken(), lexems => new LinePositionCalculator(lexems))).As<TokenWalker>();
            builder.RegisterType<Parser>().AsSelf();
        }
    }
}
