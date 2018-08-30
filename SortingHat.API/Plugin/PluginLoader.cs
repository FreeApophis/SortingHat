using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SortingHat.API.Plugin
{
    public class PluginLoader
    {
        public static void Load(string path)
        {
            Console.WriteLine($"Load Plugins from: {path}");

            List<IPlugin> plugins = new List<IPlugin>();
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles("*Plugin.dll"))
            {
                Console.WriteLine($"Plugin dll found: {file.Name}");
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface(nameof(IPlugin)) == typeof(IPlugin) && type.IsAbstract == false)
                    {
                        Console.WriteLine($"Create instance");
                        IPlugin plugin = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as IPlugin;

                        Console.WriteLine($"Plugin '{plugin.Name}' successfully loaded. ");
                        plugins.Add(plugin);
                    }
                }
            }

            foreach (var plugin in plugins)
            {
                plugin.Execute();
            }
        }
    }
}
