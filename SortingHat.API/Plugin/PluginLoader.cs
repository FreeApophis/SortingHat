using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SortingHat.API.Plugin
{
    class PluginLoader
    {
        void Load(string path)
        {
            List<IPlugin> objects = new List<IPlugin>();
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(IPlugin)) && type.IsAbstract == false)
                    {
                        IPlugin b = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as IPlugin;
                        objects.Add(b);
                    }
                }
            }
        }
    }
}
