using Autofac;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.Plugin.ExtractRelevantText;

[UsedImplicitly]
internal class RelevantInformationModule : Autofac.Module, IPlugin
{
    public string Name => "Relevant Information Extractor";
    public string Description => "Finds the most frequent words in text files, with the scan command.";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FolderScanner>().AsSelf();
        builder.RegisterType<ScanCommand>().As<IDeclaredCommand>();

        RegisterSupportedTags(builder);
    }

    private static void RegisterSupportedTags(ContainerBuilder builder)
    {
    }
}
