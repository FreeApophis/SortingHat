﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.API.Plugin
{
    [UsedImplicitly]
    public class PluginLoader : IPluginLoader
    {
        public List<IPlugin> Plugins { get; } = new List<IPlugin>();
        public List<ICommand> Commands { get; } = new List<ICommand>();

        private readonly ILogger<PluginLoader> _logger;

        public PluginLoader(ILogger<PluginLoader> logger)
        {
            _logger = logger;
        }

        public void Load(string path)
        {
            _logger.LogTrace($"Load Plugins from: {path}");

            var directoryInfo = new DirectoryInfo(path);

            foreach (var file in directoryInfo.GetFiles("*Plugin.dll"))
            {
                _logger.LogTrace($"Plugin dll found: {file.Name}");

                Assembly assembly = Assembly.LoadFrom(file.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface(nameof(IPlugin)) == typeof(IPlugin) && type.IsAbstract == false)
                    {
                        IPlugin plugin = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as IPlugin;
                        _logger.LogInformation($"Plugin '{plugin.Name}' successfully loaded. ");
                        Plugins.Add(plugin);
                    }
                }
            }
        }
    }
}