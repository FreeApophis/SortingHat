using SortingHat.API.Plugin;
using System;
using System.Reflection;
using JetBrains.Annotations;

namespace SortingHat.Plugin.MoveOnTag
{
    [UsedImplicitly]
    public class MoveOnTagModule : Autofac.Module, IPlugin
    {
        public string Name => "Move on tag";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public string Description => "This plugin can automatically move a file, when a certain tag-mask is set for the file.";
    }
}
