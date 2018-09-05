using Autofac;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System.IO;
using System.Reflection;
using System;

namespace SortingHat.Plugin.FileType
{
    class FileTypeModule : Autofac.Module, IPlugin
    {
        private const string SignatureResource = "SortingHat.Plugin.FileType.Resources.FileSignatures.csv";

        public string Name => "File Type Plugin";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically can identify file type accoridng to their content, and als create automatically tags on file type.";

        public bool Execute()
        {
            return true;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IdentifyCommand>().As<ICommand>();
            builder.RegisterType<FileTypeTaggerCommand>().As<ICommand>();
            builder.RegisterType<FileTypeFinder>().As<IFileTypeFinder>().SingleInstance();
            
            LoadDetectors(builder);
        }

        private Stream SignatureStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(SignatureResource);
        }

        private void LoadDetectors(ContainerBuilder builder)
        {
            TextReader reader = new StreamReader(SignatureStream());

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#") == false)
                {
                    builder.RegisterInstance(FileTypeDetectorFactory.Create(line)).As<IFileTypeDetector>();
                }
            }
        }
    }
}
