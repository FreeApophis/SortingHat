using System;
using System.IO;
using Autofac;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.Test.Plugin;

/// <summary>
/// A plugin that fails on purpose, for the tests of the plugin loader.
/// How it fails depends on the name of its assembly file, which the test project's PluginFixtures target chooses.
/// </summary>
[UsedImplicitly]
public sealed class FailingModule : Module, IPlugin
{
    public const string ThrowsInConstructor = nameof(ThrowsInConstructor);
    public const string ThrowsInLoad = nameof(ThrowsInLoad);
    public const string Reason = "broken on purpose";

    public FailingModule()
    {
        if (Scenario == ThrowsInConstructor)
        {
            throw new InvalidOperationException(Reason);
        }
    }

    public string Name => "Failing Test Plugin";

    public string Description => "Fails on purpose.";

    private static string Scenario => Path.GetFileNameWithoutExtension(typeof(FailingModule).Assembly.Location);

    protected override void Load(ContainerBuilder builder)
    {
        // registers something before it fails, nothing of it may stay registered
        builder.RegisterType<HalfLoadedCommand>().As<IDeclaredCommand>();

        if (Scenario == ThrowsInLoad)
        {
            throw new InvalidOperationException(Reason);
        }
    }
}
