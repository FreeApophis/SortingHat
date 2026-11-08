using System;
using System.IO;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using SortingHat.Plugin.FileType.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType;

[UsedImplicitly]
internal class FileTypeModule : Autofac.Module, IPlugin
{
    private const string SignatureResource = "SortingHat.Plugin.FileType.Resources.FileSignatures.csv";

    public string Name => "File Type Plugin";
    public string Description => "Identifies the type of a file from its content, with the identify command and the FileType variables for tag masks.";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FileTypeFinder>().As<IFileTypeFinder>().SingleInstance();

        builder.RegisterType<IdentifyCommand>().As<IDeclaredCommand>();

        builder.RegisterType<FileTypeCategoryAutoTag>().As<IAutoTag>().SingleInstance();
        builder.RegisterType<FileTypeAutoTag>().As<IAutoTag>().SingleInstance();
        builder.RegisterType<FileTypeExtensionAutoTag>().As<IAutoTag>().SingleInstance();

        builder.RegisterInstance(LoadSignatures());
    }

    private static Signatures LoadSignatures()
    {
        using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SignatureResource)
            ?? throw new InvalidOperationException($"Embedded resource '{SignatureResource}' is missing.");
        using var reader = new StreamReader(resource);

        // the signatures are part of the plugin, a broken row is a bug: the plugin does not load and hat says why
        return Signatures.Parse(reader.ReadToEnd())
            .GetOrElse(reason => throw new InvalidOperationException($"FileSignatures.csv is broken, {reason}."));
    }
}
