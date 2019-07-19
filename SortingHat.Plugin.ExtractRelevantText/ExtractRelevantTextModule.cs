using Autofac;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System;
using System.Reflection;
using SortingHat.API.AutoTag;

namespace SortingHat.Plugin.ExtractRelevant
{
    [UsedImplicitly]
    internal class RelevantInformationModule : Autofac.Module, IPlugin
    {
        public string Name => "Relevant Information Extractor";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their text content (txt).";

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FolderScanner>().AsSelf();
            builder.RegisterType<ScanCommand>().As<ICommand>();

            RegisterSupportedTags(builder);
        }

        private static void RegisterSupportedTags(ContainerBuilder builder)
        {
        }
    }

}

