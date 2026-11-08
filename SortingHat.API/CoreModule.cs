using System;
using System.IO;
using System.Security.Cryptography;
using Autofac;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.API;

/// <summary>
/// Registers the core of SortingHat: the query parser, tag parsing, auto-tags, hashing, the models and the plugin loader.
/// A host registers this module, its database module and what is its own: the console or window, logging, configuration and commands.
/// </summary>
public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Parser.Parser.Create());

        builder.RegisterType<AutoTagHandler>().As<IAutoTagHandler>();
        builder.RegisterType<FilePathExtractor>().As<IFilePathExtractor>().SingleInstance();

        builder.RegisterType<FileScanner>().As<IFileScanner>();

        // one hash algorithm per file, an instance must not be used concurrently
        builder.Register(_ => SHA256.Create()).As<HashAlgorithm>().InstancePerDependency().ExternallyOwned();
        builder.Register(context => new HashService(context.Resolve<Func<HashAlgorithm>>(), nameof(SHA256))).As<IHashService>().SingleInstance();

        builder.RegisterType<CreatedAtAutoTag>().As<IAutoTag>().SingleInstance();
        builder.RegisterType<UpdatedAtAutoTag>().As<IAutoTag>().SingleInstance();
        builder.RegisterType<FolderFromRootAutoTag>().As<IAutoTag>().SingleInstance();
        builder.RegisterType<FolderFromFileAutoTag>().As<IAutoTag>().SingleInstance();

        builder.RegisterType<PluginLoader>().As<IPluginLoader>().SingleInstance();
        builder.RegisterInstance(new PluginDirectory(Path.Combine(AppContext.BaseDirectory, "plugins")));
        builder.RegisterInstance(ContractAssemblies.Current);
    }
}
