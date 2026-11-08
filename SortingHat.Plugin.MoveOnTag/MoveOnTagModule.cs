using Autofac;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Events;
using SortingHat.API.Plugin;

namespace SortingHat.Plugin.MoveOnTag;

[UsedImplicitly]
public class MoveOnTagModule : Module, IPlugin
{
    public string Name => "Move on tag";

    public string Description => "Moves a file into a folder when it gets a tag with a move rule, see 'move-rule'.";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<MoveRuleStore>().AsSelf();
        builder.RegisterType<MoveRuleCommand>().As<IDeclaredCommand>();
        builder.RegisterType<MoveRulesCommand>().As<IDeclaredCommand>();
        builder.RegisterType<MoveOnTagHandler>().As<IFileEventHandler<FileTagged>>();
    }
}
