using Autofac;
using SortingHat.API.DI;
using System;

namespace ExifTaggerPlugin
{
    class ExifTaggerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Console.WriteLine("Exif Tagger Module Loading...");
            builder.RegisterType<ExifTaggerCommand>().As<ICommand>();
        }
    }
}
